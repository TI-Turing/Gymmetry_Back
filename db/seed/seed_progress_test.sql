/*
Seed ficticio para probar endpoints de progreso (summary / multi / details)
Ajusta @UserId y @RoutineTemplateId si cambian. El script:
 - Detecta RoutineDayId y ExerciseId asociados a la plantilla dada (ahora vía RoutineDay / DailyExercise, no RoutineExercise)
 - Genera sesiones (Daily) de los últimos 35 días (aprox 5 semanas) con porcentajes variados
 - Inserta ejercicios (DailyExercise) ligados a cada Daily (subset aleatorio determinista)
 - Inserta evaluaciones físicas (PhysicalAssessment) baseline y reciente

Puedes ejecutar múltiples veces: evita duplicados por (UserId, StartDate) y marca registros con Ip = 'seed-progress-test'.
*/
SET NOCOUNT ON;

DECLARE @UserId UNIQUEIDENTIFIER = '71ffea44-3dff-46f6-b750-4ed4d7bd4fbf';
DECLARE @RoutineTemplateId UNIQUEIDENTIFIER = 'a1111111-0000-4000-8000-000000000011';
DECLARE @Today DATE = CAST(GETUTCDATE() AS DATE);
DECLARE @StartWindow DATE = DATEADD(DAY, -34, @Today); -- 35 días hacia atrás

-- Obtener RoutineDays de la plantilla
IF OBJECT_ID('tempdb..#RoutineDays') IS NOT NULL DROP TABLE #RoutineDays;
SELECT rd.Id AS RoutineDayId,
       ROW_NUMBER() OVER(ORDER BY rd.DayNumber, rd.Id) AS RN
INTO #RoutineDays
FROM RoutineDay rd
WHERE rd.RoutineTemplateId = @RoutineTemplateId;

-- Obtener Exercises asociados a la plantilla
-- Se usan primero los ExerciseId definidos directamente en RoutineDay.ExerciseId.
-- Además, si existen Daily previos ligados a esos RoutineDay, se incluyen ejercicios de sus DailyExercise.
IF OBJECT_ID('tempdb..#Exercises') IS NOT NULL DROP TABLE #Exercises;
WITH BaseExercises AS (
    SELECT rd.ExerciseId
    FROM RoutineDay rd
    WHERE rd.RoutineTemplateId = @RoutineTemplateId AND rd.ExerciseId IS NOT NULL
    UNION
    SELECT de.ExerciseId
    FROM RoutineDay rd
    JOIN Daily d ON d.RoutineDayId = rd.Id
    JOIN DailyExercise de ON de.DailyId = d.Id
    WHERE rd.RoutineTemplateId = @RoutineTemplateId AND de.ExerciseId IS NOT NULL
)
SELECT DISTINCT TOP (12) be.ExerciseId,
       ROW_NUMBER() OVER(ORDER BY be.ExerciseId) AS RN
INTO #Exercises
FROM BaseExercises be;

IF (SELECT COUNT(*) FROM #RoutineDays) = 0
BEGIN
    RAISERROR('No se encontraron RoutineDay para la plantilla indicada.',16,1);
    RETURN;
END;
IF (SELECT COUNT(*) FROM #Exercises) = 0
BEGIN
    RAISERROR('No se encontraron Exercises asociados a la plantilla (RoutineDay/DailyExercise).',16,1);
    RETURN;
END;

-- Generar tabla de fechas
IF OBJECT_ID('tempdb..#Dates') IS NOT NULL DROP TABLE #Dates;
WITH d AS (
    SELECT @StartWindow AS d
    UNION ALL
    SELECT DATEADD(DAY,1,d) FROM d WHERE d < @Today
)
SELECT d AS SessionDate,
       DATEPART(WEEKDAY,d) AS WDay,
       ROW_NUMBER() OVER(ORDER BY d) AS RN
INTO #Dates
FROM d
OPTION (MAXRECURSION 1000);

-- Insert Daily (sesiones) simulando descansos: saltar ~ 1 de cada 5 días
DECLARE @Inserted INT = 0;
IF OBJECT_ID('tempdb..#ToInsert') IS NOT NULL DROP TABLE #ToInsert;
SELECT * INTO #ToInsert FROM #Dates WHERE (RN % 5) <> 0; -- ~80% días con sesión

DECLARE @Date DATE;
DECLARE cur CURSOR FAST_FORWARD FOR SELECT SessionDate FROM #ToInsert;
OPEN cur; FETCH NEXT FROM cur INTO @Date;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Daily WHERE UserId=@UserId AND CAST(StartDate AS DATE)=@Date)
    BEGIN
        DECLARE @Guid UNIQUEIDENTIFIER = NEWID();
        DECLARE @RoutineDayId UNIQUEIDENTIFIER = (SELECT TOP 1 RoutineDayId FROM #RoutineDays ORDER BY ABS(CHECKSUM(NEWID())));
        -- Porcentaje (simula progreso): más alto semanas recientes
        DECLARE @DayDiff INT = DATEDIFF(DAY, @Date, @Today);
        DECLARE @BasePct INT = CASE WHEN @DayDiff <= 7 THEN 70 WHEN @DayDiff <=14 THEN 60 WHEN @DayDiff <=21 THEN 55 ELSE 50 END;
        DECLARE @Pct INT = @BasePct + (ABS(CHECKSUM(NEWID())) % 31); -- +0..30
        IF @Pct>100 SET @Pct=100;
        INSERT INTO Daily (Id, BranchId, CreatedAt, DeletedAt, EndDate, Ip, IsActive, Percentage, RoutineDayId, StartDate, UpdatedAt, UserId)
        VALUES (@Guid, NULL, DATEADD(HOUR,10,CAST(@Date AS DATETIME2)), NULL, DATEADD(HOUR,11,CAST(@Date AS DATETIME2)), 'seed-progress-test', 1, @Pct, @RoutineDayId, DATEADD(HOUR,10,CAST(@Date AS DATETIME2)), NULL, @UserId);
        SET @Inserted += 1;

        -- Insertar ejercicios ejecutados (3 a 6)
        DECLARE @ExerciseCount INT = 3 + (ABS(CHECKSUM(NEWID())) % 4);
        INSERT INTO DailyExercise (Id, CreatedAt, DailyId, DeletedAt, ExerciseId, Ip, IsActive, Repetitions, [Set], UpdatedAt)
        SELECT TOP (@ExerciseCount)
            NEWID(), DATEADD(HOUR,10,CAST(@Date AS DATETIME2)), @Guid, NULL, e.ExerciseId, 'seed-progress-test', 1,
            CAST(8 + (ABS(CHECKSUM(NEWID())) % 8) AS VARCHAR(10)), -- reps 8-15 aprox
            CAST(3 + (ABS(CHECKSUM(NEWID())) % 2) AS VARCHAR(10)), -- sets 3-4
            NULL
        FROM #Exercises e
        ORDER BY ABS(CHECKSUM(NEWID()));
    END;
    FETCH NEXT FROM cur INTO @Date;
END;
CLOSE cur; DEALLOCATE cur;

PRINT CONCAT('Sesiones Daily insertadas: ', @Inserted);

-- Insertar evaluaciones físicas baseline (hace 60 días) y reciente (hoy) si no existen
IF NOT EXISTS (SELECT 1 FROM PhysicalAssessment WHERE UserId=@UserId AND CreatedAt < DATEADD(DAY,-40,GETUTCDATE()))
BEGIN
    INSERT INTO PhysicalAssessment (Id, Abdomen, BMI, BodyFatPercentage, Chest, CreatedAt, DeletedAt, Height, Hips, Ip, IsActive, LeftArm, LeftCalf, LeftForearm, LeftThigh, LowerBack, MuscleMass, Neck, RighArm, RightCalf, RightForearm, RightThigh, Shoulders, UpdatedAt, UpperBack, UserId, Waist, Weight, Wrist)
    VALUES (NEWID(), NULL, NULL, '22', '95', DATEADD(DAY,-60,GETUTCDATE()), NULL, '175', '98', 'seed-progress-test', 1, '32', '38', '28', '55', NULL, '38', '38', '32', '38', '28', '55', '120', NULL, NULL, @UserId, '85', '78', NULL);
END;
IF NOT EXISTS (SELECT 1 FROM PhysicalAssessment WHERE UserId=@UserId AND CAST(CreatedAt AS DATE)=@Today)
BEGIN
    INSERT INTO PhysicalAssessment (Id, Abdomen, BMI, BodyFatPercentage, Chest, CreatedAt, DeletedAt, Height, Hips, Ip, IsActive, LeftArm, LeftCalf, LeftForearm, LeftThigh, LowerBack, MuscleMass, Neck, RighArm, RightCalf, RightForearm, RightThigh, Shoulders, UpdatedAt, UpperBack, UserId, Waist, Weight, Wrist)
    VALUES (NEWID(), NULL, NULL, '20', '100', GETUTCDATE(), NULL, '175', '97', 'seed-progress-test', 1, '33', '39', '29', '56', NULL, '39', '39', '33', '39', '29', '56', '122', NULL, NULL, @UserId, '83', '76', NULL);
END;

-- Resumen de métricas insertadas
SELECT TOP 10 Id, StartDate, Percentage FROM Daily WHERE UserId=@UserId AND Ip='seed-progress-test' ORDER BY StartDate DESC;

/* Para limpiar los datos sembrados:
DELETE FROM DailyExercise WHERE DailyId IN (SELECT Id FROM Daily WHERE Ip='seed-progress-test' AND UserId=@UserId);
DELETE FROM Daily WHERE Ip='seed-progress-test' AND UserId=@UserId;
DELETE FROM PhysicalAssessment WHERE Ip='seed-progress-test' AND UserId=@UserId;
*/
