using Application.DTO;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QuestsApi.Data;

namespace Application.Services;

public class CategoryService
{
    private readonly QuestPlatformContext _context;

    public CategoryService(QuestPlatformContext context)
    {
        _context = context;
    }

    private static readonly (string Name, string Description)[] _defaultCategories = new[]
    {
        // ── 1. Ядро школьных точных наук ──────────────────────────────────────
        ("Математика",      "Алгебра, геометрия, теория вероятностей и смежные дисциплины"),
        ("Физика",          "Механика, электродинамика, оптика, термодинамика"),
        ("Химия",           "Органическая, неорганическая, аналитическая химия"),
        ("Информатика",     "Программирование, алгоритмы, базы данных, сети"),

        // ── 2. Гуманитарные науки ────────────────────────────────────────────
        ("Русский язык",    "Грамматика, пунктуация, орфография, стилистика"),
        ("Литература",      "Анализ произведений, авторский стиль, литературные направления"),
        ("История России",  "Древняя Русь, имперский, советский и современный периоды"),
        ("Всеобщая история","Мировая история от древности до XXI века"),
        ("Обществознание",  "Право, экономика, социология, политология, философия"),

        // ── 3. Естественные науки ────────────────────────────────────────────
        ("Биология",        "Ботаника, зоология, анатомия, генетика, экология"),
        ("География",       "Физическая, экономическая и политическая география"),
        ("Астрономия",      "Солнечная система, звёзды, галактики, космология"),
        ("Экология",        "Природные экосистемы, устойчивое развитие, климат"),

        // ── 4. Иностранные языки ─────────────────────────────────────────────
        ("Английский язык", "Грамматика, лексика, аудирование, чтение"),
        ("Другие языки",    "Испанский, немецкий, французский, китайский и другие"),

        // ── 5. Когнитивные и креативные компетенции ──────────────────────────
        ("Логика и мышление", "Умозаключения, логические головоломки, критическое мышление"),
        ("Творческие задачи", "Дизайн-мышление, генерация идей, синтез информации"),
        ("Таблицы и данные","Чтение графиков, диаграмм, таблиц и статистических отчётов"),

        // ── 6. Прикладные и профессиональные дисциплины ───────────────────────
        ("Медицина и здоровье", "Анатомия, физиология, гигиена, основы медицины"),
        ("Спорт и физкультура", "Физическая культура, виды спорта, здоровый образ жизни"),
        ("Технологии и инженерия", "Робототехника, электроника, материаловедение, конструирование"),
        ("Экономика и финансы", "Микро- и макроэкономика, финансовая грамотность, бизнес"),

        // ── 7. Культура и искусство ────────────────────────────────────────────
        ("Искусство и культура", "Живопись, архитектура, скульптура, театр, кино"),
        ("Музыка", "Теория музыки, история музыки, композиторы, жанры"),

        // ── 8. Человек и общество ──────────────────────────────────────────────
        ("Право", "Конституционное, гражданское, уголовное право, правовые нормы"),
        ("Психология", "Общая психология, возрастная, социальная, когнитивные процессы"),
        ("Философия", "История философии, этика, логика, учения и концепции"),

        // ── 9. Регионоведение ──────────────────────────────────────────────────
        ("Краеведение и этнография", "История и культура регионов, народные традиции, этносы"),
    };

    public async Task SeedDefaultCategoriesAsync()
    {
        var existingNames = await _context.Categories
            .AsNoTracking()
            .Select(c => c.Name)
            .ToListAsync();

        var missing = _defaultCategories
            .Where(t => !existingNames.Contains(t.Name))
            .ToList();

        if (!missing.Any())
            return;

        var categories = missing.Select(t => new Category
        {
            Id = Guid.NewGuid(),
            Name = t.Name,
            Description = t.Description
        });

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return null;

        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return null;

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();

        await _context.SaveChangesAsync();

        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }
}

