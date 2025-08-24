# Payments Module (Mercado Pago Integration)

## Endpoints
POST /api/payments/plan/preference
POST /api/payments/gymplan/preference
GET  /api/payments/status/{paymentId|externalPaymentId}
POST /api/payments/webhook/mercadopago (Webhook Mercado Pago)

## Flow (User Plan Paid)
1. Front obtiene PlanType y su precio (>0) y llama POST /payments/plan/preference con body:
   {
     "planTypeId": "<GUID>",
     "userId": "<GUID>",
     "successUrl": "https://app/.../success", (opcional)
     "failureUrl": "https://app/.../fail", (opcional)
     "pendingUrl": "https://app/.../pending" (opcional)
   }
2. Backend crea PaymentIntent (Status=Pending) y retorna preferenceId + init_point.
3. Front redirige a init_point.
4. Usuario paga. MP envía webhook a /payments/webhook/mercadopago con payment id.
5. Backend consulta MP /v1/payments/{id}, mapea estado. Si Approved:
   - Verifica no exista plan activo vigente.
   - Crea Plan (Start=UtcNow, End=UtcNow+30d-1s) y asocia CreatedPlanId.
6. Front puede consultar GET /payments/status/{id} (usar preferenceId o externalPaymentId) para conocer estado y plan creado.

## Gym Plan Flow
Igual que usuario pero endpoint /payments/gymplan/preference con body:
{
  "gymPlanSelectedTypeId": "<GUID>",
  "gymId": "<GUID>",
  "userId": "<GUID owner>",
  ... URLs opcionales
}
Ownership validado (gym.Owner_UserId == userId JWT).

## Free Plans
Si el precio del PlanType o GymPlanSelectedType es null o <= 0, el backend crea el plan directamente sin preferencia.

## Idempotencia
Webhook revisa si PaymentIntent ya está Approved y tiene CreatedPlanId. Si sí, responde already-processed.

## Estados soportados
Pending, Approved, Rejected, Cancelled, Expired.

## Environment Variables
MP_ACCESS_TOKEN=<<mercado pago server token>>
APP_BASE_URL_PAGOS=https://<host>/api
MP_WEBHOOK_SECRET=(opcional, si se implementa firma)

## Configuration (appsettings.json)
"Payments": {
  "BaseSuccessUrl": "https://app/success",
  "BaseFailureUrl": "https://app/failure",
  "BasePendingUrl": "https://app/pending"
}

## Test Scenarios (To implement in xUnit)
1. Preference creation persists PaymentIntent Pending.
2. Webhook Approved creates plan once (duplicate webhook idempotent).
3. Rejected payment updates status, no plan created.
4. Free plan request creates plan directly (no PaymentIntent).
5. Security: user mismatched userId body vs JWT => 400.
6. Gym ownership mismatch => 403.
7. Duplicate pending preference prevented.

## Curl Examples
# Crear preferencia plan usuario
curl -X POST https://host/api/payments/plan/preference -H "Authorization: Bearer <token>" -H "Content-Type: application/json" -d '{"planTypeId":"GUID","userId":"GUID"}'

# Webhook (simulación)
curl -X POST "https://host/api/payments/webhook/mercadopago?data.id=123456789"

# Consultar estado
curl -H "Authorization: Bearer <token>" https://host/api/payments/status/<preferenceId>

## Pending Work
- Implementar firma webhook (VerifyWebhookSignature).
- Tests automatizados.
- Migrar Wompi si se requiere.
