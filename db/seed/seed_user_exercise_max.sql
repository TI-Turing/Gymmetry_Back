/*
Seed de PR (UserExerciseMax) para el usuario 71ffea44-3dff-46f6-b750-4ed4d7bd4fbf
- Inserta al menos 10 registros.
- Incluye "antes y después" (progresión) para algunos ejercicios (Bench Press, Squat, Deadlift).
- Toma ExerciseId de la tabla Exercise (nombre singular por convención EF Core por defecto) – ajustar si en tu BD se llama distinto.
- Idempotente: no duplica si ya existe un registro con mismo UserId + ExerciseId + AchievedAt.
Ajusta nombres si tu catálogo de ejercicios difiere.

NOTA: El error anterior indicaba que 'Exercise' era tabla (no SP). Por lo tanto la tabla se llama 'Exercise', no 'Exercises'. Ajustado a singular.
*/
SET NOCOUNT ON;
DECLARE @UserId UNIQUEIDENTIFIER = '71ffea44-3dff-46f6-b750-4ed4d7bd4fbf';

-- Validar existencia de la tabla Exercise
IF OBJECT_ID('dbo.Exercise','U') IS NULL
BEGIN
    RAISERROR('La tabla dbo.Exercise no existe. Ajusta el nombre de tabla en el script.',16,1);
    RETURN;
END;

-- Obtener IDs de ejercicios por nombre (fallback a TOP n si no existen los nombres buscados)
IF OBJECT_ID('tempdb..#Ex') IS NOT NULL DROP TABLE #Ex;
WITH Named AS (
    SELECT TOP (1) Id, 'Bench Press'   AS Alias FROM Exercise WHERE Name LIKE '%bench%' ORDER BY Name
    UNION ALL SELECT TOP (1) Id, 'Squat'        FROM Exercise WHERE Name LIKE '%squat%' ORDER BY Name
    UNION ALL SELECT TOP (1) Id, 'Deadlift'     FROM Exercise WHERE Name LIKE '%dead%' ORDER BY Name
    UNION ALL SELECT TOP (1) Id, 'Overhead Press' FROM Exercise WHERE Name LIKE '%overhead%' OR Name LIKE '%shoulder press%' ORDER BY Name
    UNION ALL SELECT TOP (1) Id, 'Row'          FROM Exercise WHERE Name LIKE '%row%' ORDER BY Name
    UNION ALL SELECT TOP (1) Id, 'Pull Up'      FROM Exercise WHERE Name LIKE '%pull up%' OR Name LIKE '%dominada%' ORDER BY Name
    UNION ALL SELECT TOP (1) Id, 'Bicep Curl'   FROM Exercise WHERE Name LIKE '%curl%' ORDER BY Name
    UNION ALL SELECT TOP (1) Id, 'Tricep Extension' FROM Exercise WHERE Name LIKE '%tricep%' ORDER BY Name
    UNION ALL SELECT TOP (1) Id, 'Leg Press'    FROM Exercise WHERE Name LIKE '%leg press%' ORDER BY Name
    UNION ALL SELECT TOP (1) Id, 'Lat Pulldown' FROM Exercise WHERE Name LIKE '%lat pull%' OR Name LIKE '%pulldown%' ORDER BY Name
), Fallback AS (
    SELECT Id, CONCAT('Fallback-', ROW_NUMBER() OVER(ORDER BY Id)) AS Alias
    FROM Exercise WHERE Id NOT IN (SELECT Id FROM Named) )
SELECT * INTO #Ex FROM (
    SELECT * FROM Named
    UNION ALL
    SELECT TOP (10 - (SELECT COUNT(*) FROM Named)) * FROM Fallback
) X;

-- Debug opcional
-- SELECT * FROM #Ex;

/*
Lista planificada de PRs (EjercicioAlias, Fecha, PesoKg)
Progresión: Bench Press (70 -> 75 -> 80), Squat (100 -> 110 -> 120), Deadlift (120 -> 130)
Otros ejercicios: 1 registro cada uno
*/
IF OBJECT_ID('tempdb..#SeedPR') IS NOT NULL DROP TABLE #SeedPR;
CREATE TABLE #SeedPR(Alias NVARCHAR(64), AchievedAt DATE, WeightKg DECIMAL(10,2));
INSERT INTO #SeedPR VALUES
 ('Bench Press',  DATEADD(DAY,-60, CAST(GETUTCDATE() AS DATE)), 70),
 ('Bench Press',  DATEADD(DAY,-30, CAST(GETUTCDATE() AS DATE)), 75),
 ('Bench Press',  DATEADD(DAY,-7 , CAST(GETUTCDATE() AS DATE)), 80),
 ('Squat',        DATEADD(DAY,-55, CAST(GETUTCDATE() AS DATE)),100),
 ('Squat',        DATEADD(DAY,-28, CAST(GETUTCDATE() AS DATE)),110),
 ('Squat',        DATEADD(DAY,-5 , CAST(GETUTCDATE() AS DATE)),120),
 ('Deadlift',     DATEADD(DAY,-50, CAST(GETUTCDATE() AS DATE)),120),
 ('Deadlift',     DATEADD(DAY,-10, CAST(GETUTCDATE() AS DATE)),130),
 ('Overhead Press',DATEADD(DAY,-14, CAST(GETUTCDATE() AS DATE)),55),
 ('Row',          DATEADD(DAY,-9 , CAST(GETUTCDATE() AS DATE)),85),
 ('Pull Up',      DATEADD(DAY,-21, CAST(GETUTCDATE() AS DATE)),0),   -- peso corporal (0 como marcador)
 ('Bicep Curl',   DATEADD(DAY,-12, CAST(GETUTCDATE() AS DATE)),22.5),
 ('Tricep Extension',DATEADD(DAY,-11,CAST(GETUTCDATE() AS DATE)),35),
 ('Leg Press',    DATEADD(DAY,-8 , CAST(GETUTCDATE() AS DATE)),220),
 ('Lat Pulldown', DATEADD(DAY,-6 , CAST(GETUTCDATE() AS DATE)),75);

-- Insertar PRs si no existen (clave lógica: User + Exercise + AchievedAt)
DECLARE @Now DATETIME2 = GETUTCDATE();
INSERT INTO UserExerciseMax (Id, UserId, ExerciseId, WeightKg, AchievedAt, Ip, IsActive, CreatedAt)
SELECT NEWID(), @UserId, ex.Id, s.WeightKg, s.AchievedAt, 'seed-user-pr', 1, @Now
FROM #SeedPR s
JOIN #Ex ex ON ex.Alias = s.Alias
WHERE NOT EXISTS (
    SELECT 1 FROM UserExerciseMax u
    WHERE u.UserId = @UserId
      AND u.ExerciseId = ex.Id
      AND CAST(u.AchievedAt AS DATE) = s.AchievedAt
      AND u.IsActive = 1
);

PRINT CONCAT('PR insertados: ', @@ROWCOUNT);

-- Ver últimos PR insertados
SELECT TOP 50 * FROM UserExerciseMax WHERE UserId=@UserId ORDER BY AchievedAt DESC;

/* Limpieza (opcional)
DELETE FROM UserExerciseMax WHERE UserId=@UserId AND Ip='seed-user-pr';
*/
