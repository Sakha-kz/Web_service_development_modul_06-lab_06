using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Models;

namespace TaskManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private static readonly List<TaskItem> _tasks = new()
    {
        new TaskItem
        {
            Id = 1,
            Title = "Изучить основы JWT",
            Description = "Разобрать структуру Header, Payload, Signature и алгоритм HS256",
            IsCompleted = true,
            CreatedBy = "System"
        },
        new TaskItem
        {
            Id = 2,
            Title = "Настроить Authentication Middleware",
            Description = "Подключить AddJwtBearer и UseAuthentication в Program.cs",
            IsCompleted = true,
            CreatedBy = "System"
        },
        new TaskItem
        {
            Id = 3,
            Title = "Реализовать эндпоинт логина",
            Description = "Создать POST /api/auth/login с генерацией валидного JWT токена",
            IsCompleted = true,
            CreatedBy = "System"
        },
        new TaskItem
        {
            Id = 4,
            Title = "Защитить мутирующие методы API",
            Description = "Установить атрибут [Authorize] на POST и DELETE методы TasksController",
            IsCompleted = false,
            CreatedBy = "System"
        }
    };

    /// <summary>
    /// Получение всех задач (доступ без авторизации)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetAll()
    {
        return Ok(new
        {
            total = _tasks.Count,
            tasks = _tasks
        });
    }

    /// <summary>
    /// Получение задачи по ID (доступ без авторизации)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public IActionResult GetById(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task == null)
        {
            return NotFound(new { message = $"Задача с ID {id} не найдена." });
        }
        return Ok(task);
    }

    /// <summary>
    /// Создание новой задачи (требуется JWT токен).
    /// Задание 7: Извлечение Claim Name из HttpContext.User.Claims.
    /// </summary>
    [HttpPost]
    [Authorize]
    public IActionResult Create([FromBody] TaskItem newTask)
    {
        if (newTask == null || string.IsNullOrWhiteSpace(newTask.Title))
        {
            return BadRequest(new { message = "Название задачи обязательно для заполнения." });
        }

        // Задание 7: Чтение Claim Name из HttpContext.User.Claims
        var nameClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "unique_name")?.Value
                        ?? HttpContext.User.Identity?.Name
                        ?? "Неизвестный пользователь";

        var roleClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
                        ?? "Без роли";

        var newId = _tasks.Any() ? _tasks.Max(t => t.Id) + 1 : 1;
        newTask.Id = newId;
        newTask.CreatedBy = nameClaim;
        _tasks.Add(newTask);

        return CreatedAtAction(nameof(GetById), new { id = newTask.Id }, new
        {
            message = "Задача успешно создана аутентифицированным пользователем!",
            authorClaimName = nameClaim,
            authorClaimRole = roleClaim,
            task = newTask
        });
    }

    /// <summary>
    /// Удаление задачи (требуется JWT токен)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult Delete(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task == null)
        {
            return NotFound(new { message = $"Задача с ID {id} не найдена." });
        }

        var operatorName = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                           ?? HttpContext.User.Identity?.Name
                           ?? "Пользователь";

        _tasks.Remove(task);

        return Ok(new
        {
            message = $"Задача с ID {id} успешно удалена.",
            deletedBy = operatorName,
            deletedTask = task
        });
    }
}
