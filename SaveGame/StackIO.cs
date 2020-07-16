using System;
using System.IO;
using System.Text;

namespace Industropolis.Sim.SaveGame
{
    public ref struct StackEntry
    {
        private Span<byte> _data;
        private char[] _fieldBuffer;
        private int _pos;

        public StackEntry(Span<byte> data, char[] fieldBuffer)
        {
            _pos = 0;
            _data = data;
            _fieldBuffer = fieldBuffer;
        }

        public IntVector PopIntVector() => IntVector.Parse(PopItem());

        public int PopInt() => int.Parse(PopItem());

        public bool PopBool() => bool.Parse(PopItem());

        public T PopEnum<T>() where T : struct => Enum.Parse<T>(PopItem().ToString());

        public string PopString() => PopItem().ToString();

        public bool HasItem()
        {
            var buffer = _data.Slice(_pos);
            var sep = buffer.IndexOf((byte)';');
            if (_pos >= _data.Length || sep == -1) return false;
            return true;
        }

        private Span<char> PopItem()
        {
            if (!HasItem()) throw new ArgumentException("End of stack");
            var buffer = _data.Slice(_pos);
            var sep = buffer.IndexOf((byte)';');
            var text = buffer.Slice(0, sep);
            int len = Encoding.UTF8.GetChars(text, _fieldBuffer);
            _pos += sep + 1;
            return _fieldBuffer.AsSpan().Slice(0, len);
        }
    }

    public class StackReader : IDisposable
    {
        private Stream _file;
        private byte[] _buffer;
        private char[] _fieldBuffer;
        private int _pos = -1;
        private Memory<string> _row;

        public StackReader(string file) : this(File.OpenRead(file))
        {

        }

        public StackReader(Stream file)
        {
            _file = file;
            _fieldBuffer = new char[128];
            _pos = 0;
            _buffer = new byte[_file.Length];
            _file.Read(_buffer);
        }

        public bool TryGetStack(out StackEntry stack)
        {
            if (HasStack())
            {
                stack = GetStack();
                return true;
            }
            stack = default(StackEntry);
            return false;
        }

        public bool HasStack() => _pos < _buffer.Length;

        public StackEntry GetStack()
        {
            if (!HasStack()) throw new ArgumentException("No more stacks to get");
            var buf = _buffer.AsSpan().Slice(_pos);
            var newLine = buf.IndexOf((byte)'\n');
            if (newLine > 0) buf = buf.Slice(0, newLine);
            _pos += buf.Length + 1;
            return new StackEntry(buf, _fieldBuffer);
        }

        public void Dispose()
        {
            _file.Dispose();
        }
    }

    public class StackWriter : IDisposable
    {
        private Stream _file;
        private byte[] _buffer;
        private int _pos = 0;

        public StackWriter(Stream file)
        {
            _file = file;
            _buffer = new byte[4096];
        }

        public StackWriter(string file) : this(File.Open(file, FileMode.Create))
        {

        }

        public void WriteStack(params object[] args)
        {
            PushItems(args);
            WriteStack();
        }

        public void WriteStack()
        {
            if (_pos > 0)
            {
                _buffer[_pos] = (byte)'\n';
                _file.Write(_buffer, 0, _pos + 1);
                _pos = 0;
            }
        }

        public void PushItem(object arg)
        {
            var buffer = _buffer.AsSpan();
            var str = arg.ToString().AsSpan();
            _pos += Encoding.UTF8.GetBytes(str, buffer.Slice(_pos));
            _buffer[_pos] = (byte)';';
            _pos++;
        }

        public void PushItems(params object[] args)
        {
            foreach (var arg in args) PushItem(arg);
        }

        public void Dispose() => _file.Dispose();
    }
}