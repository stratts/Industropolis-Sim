
public class Stockpile : Building {
    private Storage _storage = new Storage();

    public Stockpile() {
        Input = _storage;
        Output = _storage;
    }

    public void AddItem(Item item, int amount) {
        for (int i = 0; i < amount; i++) {
            _storage.Insert(item);
        }
    }

    public bool HasItem(Item item, int amount) {
        if (!_storage.Has(item)) return false;
        var buffer = _storage.GetBuffer(item);
        if (buffer.Buffer < amount) return false;
        return true;
    }

    public void RemoveItem(Item item, int amount) {
        for (int i = 0; i < amount; i++) {
            _storage.Remove(item);
        }
    }
}