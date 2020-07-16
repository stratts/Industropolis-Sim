using System;
using System.IO;
using System.Text;

namespace Industropolis.Sim.SaveGame
{
    public class RowReader : IDisposable
    {
        private Stream _file;
        private byte[] _buffer;
        private char[] _fieldBuffer;
        private int _pos = -1;
        private Memory<string> _row;

        public RowReader(string file) : this(File.OpenRead(file))
        {

        }

        public RowReader(Stream file)
        {
            _file = file;
            _buffer = new byte[0];
            _fieldBuffer = new char[128];
        }

        public void LoadFile()
        {
            _pos = 0;
            _buffer = new byte[_file.Length];
            _file.Read(_buffer);
        }

        public bool AtEnd => _pos >= _buffer.Length;

        public IntVector ReadIntVector() => IntVector.Parse(ReadField());

        public int ReadInt() => int.Parse(ReadField());

        public bool ReadBool() => bool.Parse(ReadField());

        public T ReadEnum<T>() where T : struct => Enum.Parse<T>(ReadField().ToString());

        public string ReadString() => ReadField().ToString();

        public bool HasField()
        {
            var buffer = _buffer.AsSpan().Slice(_pos);
            var sep = buffer.IndexOf((byte)';');
            if (AtEnd || _buffer[_pos] == (byte)'\n' || sep == -1) return false;
            return true;
        }

        private Span<char> ReadField()
        {
            if (_pos == -1) throw new ArgumentException("Get a row first");
            if (!HasField()) throw new ArgumentException("End of row or file");
            var buffer = _buffer.AsSpan().Slice(_pos);
            var sep = buffer.IndexOf((byte)';');
            var text = buffer.Slice(0, sep);
            int len = Encoding.UTF8.GetChars(text, _fieldBuffer);
            _pos += sep + 1;
            return _fieldBuffer.AsSpan().Slice(0, len);
        }

        public bool GetRow()
        {
            if (_pos == -1)
            {
                if (_file.Length == 0) return false;
                LoadFile();
                return true;
            }
            if (AtEnd) return false;
            var newLine = _buffer.AsSpan().Slice(_pos).IndexOf((byte)'\n');
            if (newLine >= 0)
            {
                _pos += newLine + 1;
                return true;
            }
            else
            {
                _pos = _buffer.Length;
                return false;
            }
        }

        public void Dispose()
        {
            _file.Dispose();
        }
    }

    public class RowWriter : IDisposable
    {
        private int _maxLine = 128;
        private Stream _file;
        private byte[] _buffer;
        private int _pos = 0;

        public RowWriter(Stream file, int numRows)
        {
            _file = file;
            _buffer = new byte[_maxLine * numRows];
        }

        public RowWriter(string file, int numRows) : this(File.Open(file, FileMode.Create), numRows)
        {

        }

        public void WriteRow(params object[] args)
        {
            StartRow();
            WriteFields(args);
        }

        public void StartRow()
        {
            if (_pos > 0)
            {
                _buffer[_pos] = (byte)'\n';
                _pos++;
            }
        }

        public void WriteField(object arg)
        {
            var buffer = _buffer.AsSpan();
            var str = arg.ToString().AsSpan();
            _pos += Encoding.UTF8.GetBytes(str, buffer.Slice(_pos));
            _buffer[_pos] = (byte)';';
            _pos++;
        }

        public void WriteFields(params object[] args)
        {
            foreach (var arg in args) WriteField(arg);
        }

        public void SaveFile()
        {
            _file.Write(_buffer, 0, _pos);
            _file.Dispose();
        }

        public void Dispose() => SaveFile();
    }
}