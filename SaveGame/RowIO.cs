using System;
using System.IO;
using System.Text;

namespace Industropolis.Sim.SaveGame
{
    public class RowReader
    {
        private string _file;
        private byte[] _buffer;
        private string[] _fieldBuffer;
        private int _pos;

        public RowReader(string file)
        {
            _file = file;
            _fieldBuffer = new string[10];
            _buffer = new byte[0];
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

        public Span<string> ReadRow()
        {
            if (AtEnd) return null;
            var buffer = _buffer.AsSpan().Slice(_pos);

            int fieldStart = 0;
            int fieldCount = 0;

            for (int i = 0; i < buffer.Length; i++)
            {
                _pos++;
                byte c = buffer[i];
                if (c == (byte)';')
                {
                    var field = buffer.Slice(fieldStart, i - fieldStart);
                    _fieldBuffer[fieldCount] = Encoding.UTF8.GetString(field);
                    fieldCount++;
                    fieldStart = i + 1;
                }
                if (c == (byte)'\n') break;
            }

            return _fieldBuffer.AsSpan(0, fieldCount);
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
            var buffer = _buffer.AsSpan();

            if (_pos > 0)
            {
                _buffer[_pos] = (byte)'\n';
                _pos++;
            }

            for (int i = 0; i < args.Length; i++)
            {
                var str = args[i].ToString().AsSpan();
                _pos += Encoding.UTF8.GetBytes(str, buffer.Slice(_pos));
                _buffer[_pos] = (byte)';';
                _pos++;
            }
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