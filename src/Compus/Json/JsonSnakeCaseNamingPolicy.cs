using System.Text;
using System.Text.Json;

namespace Compus.Json
{
    internal class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name)) { return name; }

            var atStart = true;
            StringBuilder sb = new();

            foreach (char c in name)
            {
                if (char.IsUpper(c))
                {
                    if (!atStart) { sb.Append('_'); }

                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }

                atStart = false;
            }

            return sb.ToString();
        }
    }
}
