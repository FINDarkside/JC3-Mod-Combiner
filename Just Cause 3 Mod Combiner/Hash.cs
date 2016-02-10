using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Just_Cause_3_Mod_Combiner
{
	public static class SHA256
	{
		public static string ComputeHash(string file)
		{
			using (var stream = new BufferedStream(File.OpenRead(file), 1200000))
			{
				SHA256Managed sha = new SHA256Managed();
				byte[] checksum = sha.ComputeHash(stream);
				return BitConverter.ToString(checksum).Replace("-", String.Empty);
			}

		}
	}

	public static class HashJenkins
	{
		private static uint ComputeHash(byte[] data, int index, int length, uint seed)
		{
			uint a, b, c;

			a = b = c = 0xDEADBEEF + (uint)length + seed;

			int i = index;
			while (i + 12 < length)
			{
				a += (uint)data[i++] |
					 ((uint)data[i++] << 8) |
					 ((uint)data[i++] << 16) |
					 ((uint)data[i++] << 24);
				b += (uint)data[i++] |
					 ((uint)data[i++] << 8) |
					 ((uint)data[i++] << 16) |
					 ((uint)data[i++] << 24);
				c += (uint)data[i++] |
					 ((uint)data[i++] << 8) |
					 ((uint)data[i++] << 16) |
					 ((uint)data[i++] << 24);
				a -= c;
				a ^= (c << 4) | (c >> (32 - 4));
				c += b;
				b -= a;
				b ^= (a << 6) | (a >> (32 - 6));
				a += c;
				c -= b;
				c ^= (b << 8) | (b >> (32 - 8));
				b += a;
				a -= c;
				a ^= (c << 16) | (c >> (32 - 16));
				c += b;
				b -= a;
				b ^= (a << 19) | (a >> (32 - 19));
				a += c;
				c -= b;
				c ^= (b << 4) | (b >> (32 - 4));
				b += a;
			}

			if (i < length)
			{
				a += data[i++];
			}

			if (i < length)
			{
				a += (uint)data[i++] << 8;
			}

			if (i < length)
			{
				a += (uint)data[i++] << 16;
			}

			if (i < length)
			{
				a += (uint)data[i++] << 24;
			}

			if (i < length)
			{
				b += (uint)data[i++];
			}

			if (i < length)
			{
				b += (uint)data[i++] << 8;
			}

			if (i < length)
			{
				b += (uint)data[i++] << 16;
			}

			if (i < length)
			{
				b += (uint)data[i++] << 24;
			}

			if (i < length)
			{
				c += (uint)data[i++];
			}

			if (i < length)
			{
				c += (uint)data[i++] << 8;
			}

			if (i < length)
			{
				c += (uint)data[i++] << 16;
			}

			if (i < length)
			{
				c += (uint)data[i /*++*/] << 24;
			}

			c ^= b;
			c -= (b << 14) | (b >> (32 - 14));
			a ^= c;
			a -= (c << 11) | (c >> (32 - 11));
			b ^= a;
			b -= (a << 25) | (a >> (32 - 25));
			c ^= b;
			c -= (b << 16) | (b >> (32 - 16));
			a ^= c;
			a -= (c << 4) | (c >> (32 - 4));
			b ^= a;
			b -= (a << 14) | (a >> (32 - 14));
			c ^= b;
			c -= (b << 24) | (b >> (32 - 24));

			return c;
		}

		public static uint ComputeHash(string input)
		{
			byte[] data = Encoding.ASCII.GetBytes(input);
			return ComputeHash(data, 0, data.Length, 0);
		}

		public static string ComputeHashHex(string input)
		{
			return ComputeHash(input).ToString("X");
		}

		public static uint ComputeHash(byte[] data)
		{
			return ComputeHash(data, 0, data.Length, 0);
		}

		public static string ComputeHashHex(byte[] data)
		{
			return ComputeHash(data, 0, data.Length, 0).ToString("X");
		}
	}
}
