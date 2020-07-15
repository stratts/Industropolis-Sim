using System;
using System.IO;
using System.Text;

namespace Industropolis.Sim.SaveGame
{
    public class RowReader
    {
        private string _file;
        private byte[] _buffer;
        private char[] _fieldBuffer;
        private int _pos;
        private Memory<string> _row;

        public RowReader(string file)
        {
            _file = file;
            _buffer = new byte[0];
            _fieldBuffer = new char[128];
        }

        public void LoadFile()
        {
            _pos = 0;
            using (var f = File.OpenRead(_file))
            {
                _buffer = new byte[f.Length];
                f.Read(_buffer);
            }
        }

        public bool AtEnd => _pos >= _buffer.Length;

        public IntVector ReadIntVector() => IntVector.Parse(ReadField());

        public int ReadInt() => int.Parse(ReadField());

        public bool ReadBool() => bool.Parse(ReadField());

        public T ReadEnum<T>() where T : struct => Enum.Parse<T>(ReadField().ToString());

        public string ReadString() => ReadField().ToString();

        private Span<char> ReadField()
        {
            var buffer = _buffer.AsSpan().Slice(_pos);
            var sep = buffer.IndexOf((byte)';');
            if (AtEnd || _buffer[_pos] == (byte)'\n' || sep == -1) throw new ArgumentException("End of row or file");
            var text = buffer.Slice(0, sep);
            int len = Encoding.UTF8.GetChars(text, _fieldBuffer);
            _pos += sep + 1;
            return _fieldBuffer.AsSpan().Slice(0, len);
        }

        public void NextRow()
        {
            if (AtEnd) return;
            var newLine = _buffer.AsSpan().Slice(_pos).IndexOf((byte)'\n');
            if (newLine == -1) _pos = _buffer.Length;
            else _pos += newLine + 1;
        }
    }

    public class RowWriter
    {
        private int _maxLine = 128;
        private string _file;
        private byte[] _buffer;
        private int _pos = 0;

        public RowWriter(string file, int numRows)
        {
            _file = file;
            _buffer = new byte[_maxLine * numRows];
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
            using (var f = File.Open(_file, FileMode.Create))
            {
                f.Write(_buffer, 0, _pos);
            }
        }
    }
}