using System;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Payments;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.Models;
using Gymmetry.Domain.DTO.Payments.Requests;
using Gymmetry.Repository.Services;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Gymmetry.Domain.Options;
using Xunit;

namespace Gymmetry.Tests.Payments
{
    public class PaymentsTests
    {
        private GymmetryContext BuildContext()
        {
            var options = new DbContextOptionsBuilder<GymmetryContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var ctx = new GymmetryContext(options);
            // Seed minimal PlanType & GymPlanSelectedType
            ctx.PlanTypes.Add(new PlanType { Id = Guid.NewGuid(), Name = "Plan Test", IsActive = true, Price = 1000, CreatedAt = DateTime.UtcNow });
            ctx.GymPlanSelectedTypes.Add(new GymPlanSelectedType { Id = Guid.NewGuid(), Name = "Gym Plan Test", IsActive = true, Price = 2000, CreatedAt = DateTime.UtcNow });
            ctx.SaveChanges();
            return ctx;
        }

        private IPaymentIntentRepository BuildRepo(GymmetryContext ctx) => new PaymentIntentRepository(ctx);

        private IPaymentGatewayService BuildGateway(GymmetryContext ctx, IPaymentIntentRepository repo)
        {
            var httpClient = new System.Net.Http.HttpClient(new FakeHandler());
            var options = Options.Create(new PaymentsOptions());
            return new MercadoPagoPaymentGatewayService(httpClient, NullLogger<MercadoPagoPaymentGatewayService>.Instance, options, repo, ctx);
        }

        [Fact]
        public async Task PreferenceCreation_Persists_PaymentIntent_UserPlan()
        {
            var ctx = BuildContext();
            var repo = BuildRepo(ctx);
            var gateway = BuildGateway(ctx, repo);
            var planType = ctx.PlanTypes.First();
            var userId = Guid.NewGuid();

            var pref = await gateway.CreateUserPlanPreferenceAsync(new CreateUserPlanPreferenceRequest
            {
                PlanTypeId = planType.Id,
                UserId = userId
            });

            Assert.NotNull(pref.Id);
            var intent = ctx.Payments.FirstOrDefault(p => p.PreferenceId == pref.Id);
            Assert.NotNull(intent);
            Assert.Equal(PaymentStatus.Pending, intent!.Status);
            Assert.Equal(planType.Id, intent.PlanTypeId);
        }

        [Fact]
        public async Task DuplicatePendingPreference_Prevented()
        {
            var ctx = BuildContext();
            var repo = BuildRepo(ctx);
            var gateway = BuildGateway(ctx, repo);
            var planType = ctx.PlanTypes.First();
            var userId = Guid.NewGuid();
            await gateway.CreateUserPlanPreferenceAsync(new CreateUserPlanPreferenceRequest { PlanTypeId = planType.Id, UserId = userId });
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await gateway.CreateUserPlanPreferenceAsync(new CreateUserPlanPreferenceRequest { PlanTypeId = planType.Id, UserId = userId }));
        }

        // Fake HTTP handler to emulate Mercado Pago preference creation
        private class FakeHandler : System.Net.Http.HttpMessageHandler
        {
            protected override Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                if (request.RequestUri!.AbsoluteUri.Contains("checkout/preferences"))
                {
                    var json = "{ \"id\": \"pref_test_123\", \"init_point\": \"https://pay\", \"sandbox_init_point\": \"https://sandbox\" }";
                    return Task.FromResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json")
                    });
                }
                return Task.FromResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent("{}", System.Text.Encoding.UTF8, "application/json")
                });
            }
        }
    }
}
