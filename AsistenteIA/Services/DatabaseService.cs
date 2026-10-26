using SQLite;
using AsistenteIA.Models;

namespace AsistenteIA.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _db;

    // Crea la conexión y la tabla solo la primera vez
    private async Task InitAsync()
    {
        if (_db is not null) return;

        string path = Path.Combine(FileSystem.AppDataDirectory, "asistente.db3");
        _db = new SQLiteAsyncConnection(path);
        await _db.CreateTableAsync<ConversationRecord>();
    }

    public async Task<int> SaveAsync(ConversationRecord record)
    {
        await InitAsync();
        return await _db!.InsertAsync(record);
    }

    public async Task<List<ConversationRecord>> GetAllAsync()
    {
        await InitAsync();
        return await _db!.Table<ConversationRecord>()
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<List<ConversationRecord>> SearchAsync(string text)
    {
        await InitAsync();
        return await _db!.Table<ConversationRecord>()
            .Where(r => r.Prompt.Contains(text) || r.Answer.Contains(text))
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<ConversationRecord?> GetByIdAsync(int id)
    {
        await InitAsync();
        return await _db!.Table<ConversationRecord>()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<int> UpdateAsync(ConversationRecord record)
    {
        await InitAsync();
        return await _db!.UpdateAsync(record);
    }

    public async Task<int> DeleteAsync(ConversationRecord record)
    {
        await InitAsync();
        return await _db!.DeleteAsync(record);
    }
}