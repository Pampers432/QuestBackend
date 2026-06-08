using Application.DTO;
using ClosedXML.Excel;
using Domain.Entities;
using Application.Interfaces;
using QuestsApi;
using QuestsApi.DTO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class QuestSessionService
    {
        private readonly QuestRepository _questRepository;
        private readonly IQuestNotifier _notifier;

        public QuestSessionService(QuestRepository questRepository, IQuestNotifier notifier)
        {
            _questRepository = questRepository;
            _notifier = notifier;
        }

        public async Task<QuestSession> CreateSessionAsync(QuestSession session)
        {
            await _questRepository.CreateSessionAsync(session);

            return session;
        }

        public async Task<QuestSessionDetailsDto?> GetByAccessCodeAsync(string accessCode)
        {
            var session = await _questRepository.GetByAccessCodeAsync(accessCode);

            if (session == null)
            {
                return null;
            }

            return new QuestSessionDetailsDto(
                Id: session.Id,
                QuestId: session.QuestId,
                StartedBy: session.StartedBy,
                StartedByUsername: session.StartedByNavigation.Username,
                TimeLimit: session.TimeLimit,
                AllowPartialCompletion: session.AllowPartialCompletion,
                AllowToSkip: session.AllowToSkip,
                AccessCode: session.AccessCode,
                IsActive: session.EndsAt == null || session.EndsAt > DateTime.UtcNow,
                StartsAt: session.StartsAt,
                EndsAt: session.EndsAt,
                    Quest: new QuestDto(
                        Id: session.Quest.Id,
                        Title: session.Quest.Title,
                        Description: session.Quest.Description,
                        Subject: session.Quest.Subject,
                        Difficulty: session.Quest.Difficulty,
                        Status: session.Quest.Status,
                        Visibility: session.Quest.Visibility,
                        AuthorId: session.Quest.AuthorId,
                    Author: new UserDto(
                        Id: session.Quest.Author.Id,
                        Username: session.Quest.Author.Username,
                        Role: session.Quest.Author.Role,
                        IsBlocked: session.Quest.Author.IsBlocked
                    ),
                    CategoryId: session.Quest.CategoryId,
                    Category: session.Quest.Category != null ? new CategoryDto(
                        Id: session.Quest.Category.Id,
                        Name: session.Quest.Category.Name,
                        Description: session.Quest.Category.Description
                    ) : null,
                    QuestRooms: session.Quest.QuestRooms
                        .OrderBy(r => r.OrderIndex)
                        .Select(r => new QuestRoomDto(
                            Id: r.Id,
                            QuestId: r.QuestId,
                            RoomTemplateId: r.RoomTemplateId,
                            Title: r.Title,
                            OrderIndex: r.OrderIndex,
                            Questions: r.Questions
                                .OrderBy(q => q.OrderIndex)
                                .Select(q => new QuestionDto(
                                    Id: q.Id,
                                    QuestRoomId: q.QuestRoomId,
                                    TargetObject: q.TargetObject,
                                    Type: q.Type,
                                    Text: q.Text,
                                    Attachment: q.Attachment,
                                    Points: q.Points,
                                    Hint: q.Hint,
                                    OrderIndex: q.OrderIndex,
                                    AnswerOptions: q.AnswerOptions
                                        .OrderBy(a => a.OrderIndex)
                                        .Select(a => new AnswerOptionDto(
                                            Id: a.Id,
                                            QuestionId: a.QuestionId,
                                            Text: a.Text,
                                            IsCorrect: a.IsCorrect,
                                            OrderIndex: a.OrderIndex,
                                            MatchPair: a.MatchPair,
                                            SequenceOrder: a.SequenceOrder,
                                            Attachment: a.Attachment
                                        )).ToList()
                                )).ToList(),
                            RoomTemplate: new RoomTemplateDto(
                                Id: r.RoomTemplate.Id,
                                Name: r.RoomTemplate.Name,
                                PreviewImageUrl: r.RoomTemplate.PreviewImage,
                                SceneData: r.RoomTemplate.SceneData
                            )
                        )).ToList()
                ),
                Attempts: session.Attempts
                    .OrderByDescending(a => a.StartedAt)
                    .Select(a => new QuestSessionAttemptDto(
                        Id: a.Id,
                        UserId: a.UserId,
                        Username: a.User.Username,
                        Status: a.Status,
                        Score: a.Score,
                        MaxScore: a.MaxScore,
                        StartedAt: a.StartedAt,
                        FinishedAt: a.FinishedAt,
                        UserAnswers: a.UserAnswers
                            .Select(ua => new QuestSessionAttemptUserAnswerDto(
                                Id: ua.Id,
                                AttemptId: ua.AttemptId,
                                QuestionId: ua.QuestionId,
                                AnswerData: ua.AnswerData,
                                IsCorrect: ua.IsCorrect,
                                PointsAwarded: ua.PointsAwarded,
                                AnsweredAt: ua.AnsweredAt
                            )).ToList()
                    )).ToList()
            );
        }

        public async Task<Attempt> StartAttemptAsync(StartAttemptDto dto)
        {
            var attempt = new Attempt
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                QuestSessionId = dto.QuestSessionId,
                Status = "in_progress",
                StartedAt = DateTime.UtcNow,
                Score = 0,
                MaxScore = 0
            };

            await _questRepository.AddAttemptAsync(attempt);
            return attempt;
        }

        public async Task FinishAttemptAsync(Guid attemptId)
        {
            var attempt = await _questRepository.GetAttemptWithAnswersAsync(attemptId);

            if (attempt == null)
                throw new Exception("Attempt not found");

            attempt.Score = attempt.UserAnswers.Sum(a => a.PointsAwarded);
            attempt.MaxScore = attempt.UserAnswers.Sum(a => a.Question.Points);

            attempt.FinishedAt = DateTime.UtcNow;
            attempt.Status = "completed";

            await _questRepository.UpdateAttemptAsync(attempt);

            // Отправляем уведомление об обновлении сессии (новый результат)
            await _notifier.NotifySessionUpdatedAsync(attempt.QuestSessionId);
        }

        public async Task SaveAnswerAsync(UserAnswerDto dto)
        {
            var answer = new UserAnswer
            {
                Id = Guid.NewGuid(),
                AttemptId = dto.AttemptId,
                QuestionId = dto.QuestionId,
                AnswerData = dto.AnswerData,
                IsCorrect = dto.IsCorrect,
                PointsAwarded = dto.PointsAwarded
            };

            await _questRepository.AddUserAnswerAsync(answer);
        }

        public async Task<SessionDashboardDto?> GetSessionDashboardAsync(Guid sessionId)
        {
            var session = await _questRepository.GetSessionByIdAsync(sessionId);
            if (session == null)
                return null;

            var attempts = session.Attempts
                .OrderByDescending(a => a.StartedAt)
                .Select(a => new AttemptSummaryDto(
                    AttemptId: a.Id,
                    UserId: a.UserId,
                    Username: a.User.Username,
                    Status: a.Status,
                    Score: a.Score,
                    MaxScore: a.MaxScore,
                    StartedAt: a.StartedAt,
                    FinishedAt: a.FinishedAt
                ))
                .ToList();

            return new SessionDashboardDto(
                SessionId: session.Id,
                AccessCode: session.AccessCode,
                QuestTitle: session.Quest.Title,
                StartsAt: session.StartsAt,
                EndsAt: session.EndsAt,
                IsActive: session.IsActive && (session.EndsAt == null || session.EndsAt > DateTime.UtcNow),
                TotalAttempts: session.Attempts.Count,
                CompletedAttempts: session.Attempts.Count(a => a.Status == "completed"),
                Attempts: attempts
            );
        }

        public async Task<byte[]?> ExportSessionReportAsync(Guid sessionId, string format)
        {
            if (format.ToLower() == "xlsx")
            {
                var fullSession = await _questRepository.GetSessionWithFullDetailsAsync(sessionId);
                if (fullSession == null) return null;
                return GenerateExcelReport(fullSession);
            }

            var session = await _questRepository.GetSessionByIdWithDetailsAsync(sessionId);
            if (session == null)
                return null;

            if (format.ToLower() == "csv")
            {
                return GenerateCsvReport(session);
            }
            else
            {
                return GenerateJsonReport(session);
            }
        }

        private byte[] GenerateExcelReport(QuestSession session)
        {
            using var workbook = new XLWorkbook();
            var orangeColor = XLColor.FromColor(Color.FromArgb(255, 107, 53));
            var skyColor = XLColor.FromColor(Color.FromArgb(69, 183, 209));
            var successColor = XLColor.FromColor(Color.FromArgb(0, 184, 148));
            var errorColor = XLColor.FromColor(Color.FromArgb(225, 112, 85));
            var headerBg = XLColor.FromColor(Color.FromArgb(45, 55, 72));
            var altRowBg = XLColor.FromColor(Color.FromArgb(247, 250, 252));

            // ========== Sheet 1: Summary ==========
            var summaryWs = workbook.Worksheets.Add("Сводка");

            summaryWs.Cell(1, 1).Value = "Отчёт по сессии квеста";
            summaryWs.Cell(1, 1).Style.Font.Bold = true;
            summaryWs.Cell(1, 1).Style.Font.FontSize = 18;
            summaryWs.Cell(1, 1).Style.Font.FontColor = orangeColor;
            summaryWs.Range(1, 1, 1, 4).Merge();

            summaryWs.Cell(3, 1).Value = "Параметр";
            summaryWs.Cell(3, 2).Value = "Значение";
            summaryWs.Range(3, 1, 3, 2).Style.Font.Bold = true;
            summaryWs.Range(3, 1, 3, 2).Style.Fill.BackgroundColor = headerBg;
            summaryWs.Range(3, 1, 3, 2).Style.Font.FontColor = XLColor.White;
            summaryWs.Range(3, 1, 3, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var infoRows = new (string, string)[]
            {
                ("Квест", session.Quest.Title),
                ("Код доступа", session.AccessCode),
                ("Дата начала", session.StartsAt.ToString("dd.MM.yyyy HH:mm")),
                ("Дата окончания", session.EndsAt?.ToString("dd.MM.yyyy HH:mm") ?? "—"),
                ("Всего попыток", session.Attempts.Count.ToString()),
                ("Завершено", session.Attempts.Count(a => a.Status == "completed").ToString()),
                ("Активна", session.IsActive ? "Да" : "Нет"),
            };

            for (int i = 0; i < infoRows.Length; i++)
            {
                var row = i + 4;
                summaryWs.Cell(row, 1).Value = infoRows[i].Item1;
                summaryWs.Cell(row, 2).Value = infoRows[i].Item2;
                summaryWs.Cell(row, 1).Style.Font.Bold = true;
                summaryWs.Cell(row, 1).Style.Fill.BackgroundColor = altRowBg;
                if (i % 2 == 0)
                {
                    summaryWs.Cell(row, 1).Style.Fill.BackgroundColor = altRowBg;
                    summaryWs.Cell(row, 2).Style.Fill.BackgroundColor = altRowBg;
                }
            }

            var completedAttempts = session.Attempts.Where(a => a.Status == "completed").ToList();
            if (completedAttempts.Any())
            {
                var avgScore = completedAttempts.Average(a => a.MaxScore > 0 ? (double)a.Score / a.MaxScore * 100 : 0);
                var maxScore = completedAttempts.Max(a => a.Score);
                var minScore = completedAttempts.Min(a => a.Score);

                int statRow = infoRows.Length + 5;
                summaryWs.Cell(statRow, 1).Value = "Статистика по завершённым попыткам";
                summaryWs.Cell(statRow, 1).Style.Font.Bold = true;
                summaryWs.Cell(statRow, 1).Style.Font.FontSize = 14;
                summaryWs.Cell(statRow, 1).Style.Font.FontColor = skyColor;
                summaryWs.Range(statRow, 1, statRow, 2).Merge();

                var stats = new (string, string)[]
                {
                    ("Средний процент выполнения", $"{avgScore:F1}%"),
                    ("Максимальный балл", maxScore.ToString()),
                    ("Минимальный балл", minScore.ToString()),
                    ("Всего набрано баллов", completedAttempts.Sum(a => a.Score).ToString()),
                };

                for (int i = 0; i < stats.Length; i++)
                {
                    var r = statRow + 1 + i;
                    summaryWs.Cell(r, 1).Value = stats[i].Item1;
                    summaryWs.Cell(r, 2).Value = stats[i].Item2;
                    summaryWs.Cell(r, 1).Style.Font.Bold = true;
                    if (i % 2 == 0)
                    {
                        summaryWs.Cell(r, 1).Style.Fill.BackgroundColor = altRowBg;
                        summaryWs.Cell(r, 2).Style.Fill.BackgroundColor = altRowBg;
                    }
                }
            }

            summaryWs.Columns(1, 2).AdjustToContents();

            // ========== Sheet 2: Результаты (Attempts) ==========
            var resultsWs = workbook.Worksheets.Add("Результаты");

            var headers = new[] { "Ученик", "Статус", "Баллы", "Макс. баллы", "%", "Начало", "Завершение" };
            for (int i = 0; i < headers.Length; i++)
            {
                resultsWs.Cell(1, i + 1).Value = headers[i];
                resultsWs.Cell(1, i + 1).Style.Font.Bold = true;
                resultsWs.Cell(1, i + 1).Style.Fill.BackgroundColor = headerBg;
                resultsWs.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
                resultsWs.Cell(1, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                resultsWs.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            var orderedAttempts = session.Attempts.OrderByDescending(a => a.StartedAt).ToList();
            for (int i = 0; i < orderedAttempts.Count; i++)
            {
                var a = orderedAttempts[i];
                var row = i + 2;
                var pct = a.MaxScore > 0 ? Math.Round((double)a.Score / a.MaxScore * 100, 1) : 0;

                resultsWs.Cell(row, 1).Value = a.User.Username;
                resultsWs.Cell(row, 2).Value = a.Status == "completed" ? "Завершено" : "В процессе";
                resultsWs.Cell(row, 3).Value = a.Score;
                resultsWs.Cell(row, 4).Value = a.MaxScore;
                resultsWs.Cell(row, 5).Value = pct;
                resultsWs.Cell(row, 5).Style.NumberFormat.Format = "0.0";
                resultsWs.Cell(row, 6).Value = a.StartedAt.ToString("dd.MM.yyyy HH:mm");
                resultsWs.Cell(row, 7).Value = a.FinishedAt?.ToString("dd.MM.yyyy HH:mm") ?? "—";

                // Color the percentage cell
                var pctColor = pct >= 80 ? successColor : pct >= 60 ? orangeColor : errorColor;
                resultsWs.Cell(row, 5).Style.Font.FontColor = pctColor;
                resultsWs.Cell(row, 5).Style.Font.Bold = true;

                if (i % 2 == 1)
                {
                    resultsWs.Range(row, 1, row, 7).Style.Fill.BackgroundColor = altRowBg;
                }

                resultsWs.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                resultsWs.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                resultsWs.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            resultsWs.Columns(1, 7).AdjustToContents();
            resultsWs.Column(5).Width = 10;
            resultsWs.Column(1).Width = Math.Max(resultsWs.Column(1).Width, 20);

            // ========== Sheet 3: Детальный разбор ==========
            var detailWs = workbook.Worksheets.Add("Детальный разбор");

            var allQuestions = session.Quest.QuestRooms
                .OrderBy(r => r.OrderIndex)
                .SelectMany(r => r.Questions.OrderBy(q => q.OrderIndex))
                .ToList();

            // Build header: Student | Room | Question | Type | Their Answer | Correct? | Points
            var detailHeaders = new[] { "Ученик", "Комната", "Вопрос", "Тип", "Ответ ученика", "Правильный ответ", "Результат", "Баллы" };
            for (int i = 0; i < detailHeaders.Length; i++)
            {
                detailWs.Cell(1, i + 1).Value = detailHeaders[i];
                detailWs.Cell(1, i + 1).Style.Font.Bold = true;
                detailWs.Cell(1, i + 1).Style.Fill.BackgroundColor = headerBg;
                detailWs.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
                detailWs.Cell(1, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                detailWs.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int detailRow = 2;
            foreach (var attempt in orderedAttempts)
            {
                var startRow = detailRow;
                foreach (var question in allQuestions)
                {
                    var userAnswer = attempt.UserAnswers.FirstOrDefault(ua => ua.QuestionId == question.Id);
                    var isCorrect = userAnswer?.IsCorrect ?? false;

                    var roomName = session.Quest.QuestRooms
                        .FirstOrDefault(r => r.Questions.Any(q => q.Id == question.Id))?.Title
                        ?? "Комната";

                    var correctAnswerText = question.Type switch
                    {
                        "single_choice" or "multiple_choice" =>
                            string.Join(", ", question.AnswerOptions.Where(o => o.IsCorrect == true).Select(o => o.Text ?? "—")),
                        "text_input" or "number_input" =>
                            question.AnswerOptions.FirstOrDefault()?.Text ?? "—",
                        _ => "—"
                    };

                    var studentAnswer = "Нет ответа";
                    if (userAnswer?.AnswerData != null)
                    {
                        try
                        {
                            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(userAnswer.AnswerData);
                            if (data != null)
                            {
                                if (data.TryGetValue("text_answer", out var textAns))
                                    studentAnswer = textAns?.ToString() ?? "—";
                                else if (data.TryGetValue("selected_options", out var opts))
                                {
                                    var optList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(opts.ToString()!) ?? new();
                                    studentAnswer = string.Join(", ", optList.Select(id =>
                                        question.AnswerOptions.FirstOrDefault(o => o.Id.ToString() == id)?.Text ?? id));
                                    if (string.IsNullOrEmpty(studentAnswer)) studentAnswer = "—";
                                }
                            }
                        }
                        catch { studentAnswer = "Ошибка формата"; }
                    }

                    var typeNames = new Dictionary<string, string>
                    {
                        ["single_choice"] = "Один вариант",
                        ["multiple_choice"] = "Несколько вариантов",
                        ["text_input"] = "Текстовый ответ",
                        ["number_input"] = "Числовой ответ"
                    };

                    detailWs.Cell(detailRow, 1).Value = attempt.User.Username;
                    detailWs.Cell(detailRow, 2).Value = roomName;
                    detailWs.Cell(detailRow, 3).Value = question.Text;
                    detailWs.Cell(detailRow, 4).Value = typeNames.GetValueOrDefault(question.Type, question.Type);
                    detailWs.Cell(detailRow, 5).Value = studentAnswer;
                    detailWs.Cell(detailRow, 6).Value = correctAnswerText;

                    if (userAnswer != null)
                    {
                        detailWs.Cell(detailRow, 7).Value = isCorrect ? "✓" : "✗";
                        detailWs.Cell(detailRow, 7).Style.Font.FontColor = isCorrect ? successColor : errorColor;
                        detailWs.Cell(detailRow, 7).Style.Font.Bold = true;
                        detailWs.Cell(detailRow, 8).Value = userAnswer.PointsAwarded;
                    }
                    else
                    {
                        detailWs.Cell(detailRow, 7).Value = "—";
                        detailWs.Cell(detailRow, 8).Value = 0;
                    }

                    detailRow++;
                }

                // Add a separator with user total
                var totalRow = detailRow;
                detailWs.Cell(totalRow, 1).Value = attempt.User.Username;
                detailWs.Cell(totalRow, 3).Value = $"Итого: {attempt.Score} / {attempt.MaxScore}";
                detailWs.Cell(totalRow, 3).Style.Font.Bold = true;
                detailWs.Cell(totalRow, 7).Value = attempt.Status == "completed" ? "Завершено" : "В процессе";
                detailWs.Cell(totalRow, 7).Style.Font.Italic = true;
                detailWs.Range(totalRow, 1, totalRow, 8).Style.Fill.BackgroundColor = XLColor.FromColor(Color.FromArgb(255, 245, 240));
                detailRow += 2;
            }

            detailWs.Columns(1, 8).AdjustToContents();
            detailWs.Column(3).Width = Math.Min(Math.Max(detailWs.Column(3).Width, 40), 60);
            detailWs.Column(5).Width = Math.Min(Math.Max(detailWs.Column(5).Width, 30), 50);
            detailWs.Column(6).Width = Math.Min(Math.Max(detailWs.Column(6).Width, 30), 50);

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        private byte[] GenerateCsvReport(QuestSession session)
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, System.Text.Encoding.UTF8);

            writer.WriteLine("Username,Status,Score,MaxScore,StartedAt,FinishedAt");
            
            foreach (var attempt in session.Attempts.OrderByDescending(a => a.StartedAt))
            {
                writer.WriteLine($"{attempt.User.Username},{attempt.Status},{attempt.Score},{attempt.MaxScore},{attempt.StartedAt:yyyy-MM-dd HH:mm:ss},{attempt.FinishedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""}");
            }

            writer.Flush();
            return ms.ToArray();
        }

        private byte[] GenerateJsonReport(QuestSession session)
        {
            var report = new
            {
                SessionId = session.Id,
                AccessCode = session.AccessCode,
                QuestTitle = session.Quest.Title,
                StartsAt = session.StartsAt,
                EndsAt = session.EndsAt,
                Attempts = session.Attempts.Select(a => new
                {
                    a.Id,
                    Username = a.User.Username,
                    a.Status,
                    a.Score,
                    a.MaxScore,
                    a.StartedAt,
                    a.FinishedAt
                })
            };

            var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }
}
