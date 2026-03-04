-- Seed Data Script for Quest Platform
-- Run this script after creating the database

USE QuestPlatform;
GO

-- Clear existing data (optional, uncomment if needed)
-- DELETE FROM UserAnswers;
-- DELETE FROM Attempts;
-- DELETE FROM QuestSessions;
-- DELETE FROM Questions;
-- DELETE FROM AnswerOptions;
-- DELETE FROM QuestRooms;
-- DELETE FROM Quests;
-- DELETE FROM RoomTemplates;
-- DELETE FROM Categories;
-- DELETE FROM Users;

-- Insert Categories
INSERT INTO Categories (Id, Name, Description) VALUES
(NEWID(), N'Математика', N'Квесты по математике и алгебре'),
(NEWID(), N'Русский язык', N'Квесты по русскому языку и литературе'),
(NEWID(), N'География', N'Квесты по географии и страноведению'),
(NEWID(), N'История', N'Квесты по истории России и мира'),
(NEWID(), N'Физика', N'Квесты по физике и естественным наукам');
GO

-- Insert Users (passwords are hashed: "123456" for all)
-- Password hash format: base64(salt).base64(hash) using PBKDF2
-- For testing, using a known hash pattern (in production, use proper hashing)
INSERT INTO Users (Id, Username, PasswordHash, Role, IsBlocked) VALUES
('0EF0EA1A-7E15-402B-894F-5D7224607447', N'admin', N'$2a$11$KIXxXxXxXxXxXxXxXxXxOeXxXxXxXxXxXxXxXxXxXxXxXxXxXxXx', N'Admin', 0),
(NEWID(), N'teacher1', N'$2a$11$KIXxXxXxXxXxXxXxXxXxOeXxXxXxXxXxXxXxXxXxXxXxXxXxXxXx', N'Teacher', 0),
(NEWID(), N'student1', N'$2a$11$KIXxXxXxXxXxXxXxXxXxOeXxXxXxXxXxXxXxXxXxXxXxXxXxXxXx', N'Student', 0),
(NEWID(), N'student2', N'$2a$11$KIXxXxXxXxXxXxXxXxXxOeXxXxXxXxXxXxXxXxXxXxXxXxXxXxXx', N'Student', 0);
GO

-- Note: For proper password hashing, you should register users through the API
-- The above users are placeholders. Real passwords should be hashed via AuthService

-- Insert Room Templates
DECLARE @Template1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Template2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Template3Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO RoomTemplates (Id, Name, PreviewImage, SceneData) VALUES
(@Template1Id, N'Классная комната', N'/uploads/classroom.jpg', N'[{"name":"Door","x":50,"y":200,"w":150,"h":300},{"name":"Desk1","x":250,"y":150,"w":100,"h":80},{"name":"Board","x":400,"y":100,"w":200,"h":150}]'),
(@Template2Id, N'Библиотека', N'/uploads/library.jpg', N'[{"name":"Door","x":50,"y":200,"w":150,"h":300},{"name":"Shelf1","x":250,"y":100,"w":120,"h":200},{"name":"Shelf2","x":400,"y":100,"w":120,"h":200}]'),
(@Template3Id, N'Лаборатория', N'/uploads/lab.jpg', N'[{"name":"Door","x":50,"y":200,"w":150,"h":300},{"name":"Table1","x":250,"y":200,"w":150,"h":100},{"name":"Equipment","x":450,"y":150,"w":100,"h":150}]');
GO

-- Insert Quests (will reference categories and users)
-- First, get category and user IDs
DECLARE @MathCategoryId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Математика');
DECLARE @HistoryCategoryId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Categories WHERE Name = N'История');
DECLARE @AdminUserId UNIQUEIDENTIFIER = '0EF0EA1A-7E15-402B-894F-5D7224607447';

DECLARE @Quest1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Quest2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Room1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Room2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Answer1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Answer2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Answer3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Answer4Id UNIQUEIDENTIFIER = NEWID();

DECLARE @Template1Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM RoomTemplates WHERE Name = N'Классная комната');
DECLARE @Template2Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM RoomTemplates WHERE Name = N'Библиотека');

-- Insert Quest 1: Математика
INSERT INTO Quests (Id, Title, Description, Subject, Difficulty, Status, AuthorId, CategoryId) VALUES
(@Quest1Id, N'Основы алгебры', N'Тест по основам алгебры для начинающих', N'Математика', N'Easy', N'Published', @AdminUserId, @MathCategoryId);

-- Insert Quest Rooms
INSERT INTO QuestRooms (Id, QuestId, RoomTemplateId, Title, OrderIndex) VALUES
(@Room1Id, @Quest1Id, @Template1Id, N'Комната 1: Основы', 0),
(@Room2Id, @Quest1Id, @Template2Id, N'Комната 2: Уравнения', 1);

-- Insert Questions
INSERT INTO Questions (Id, QuestRoomId, Type, Text, Points, Hint, OrderIndex) VALUES
(@Question1Id, @Room1Id, N'single_choice', N'Чему равно 2 + 2?', 10, N'Попробуйте посчитать', 0),
(@Question2Id, @Room2Id, N'single_choice', N'Решите уравнение: x + 5 = 10', 15, N'Вычтите 5 из обеих частей', 0);

-- Insert Answer Options
INSERT INTO AnswerOptions (Id, QuestionId, Text, IsCorrect, OrderIndex) VALUES
(@Answer1Id, @Question1Id, N'3', 0, 0),
(@Answer2Id, @Question1Id, N'4', 1, 1),
(@Answer3Id, @Question1Id, N'5', 0, 2),
(@Answer4Id, @Question2Id, N'x = 5', 1, 0);

-- Insert Quest 2: История
INSERT INTO Quests (Id, Title, Description, Subject, Difficulty, Status, AuthorId, CategoryId) VALUES
(@Quest2Id, N'История России', N'Тест по истории России', N'История', N'Medium', N'Published', @AdminUserId, @HistoryCategoryId);
GO

PRINT 'Seed data inserted successfully!';
PRINT 'Note: User passwords need to be set via API registration endpoint';
PRINT 'Default test credentials should be created through /api/Auth/register';

