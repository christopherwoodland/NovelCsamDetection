using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NovelCsam.Models
{
	public record ExtractVideoFrameJsonMetaData
	{
		[JsonPropertyName("source_file")]
		public string SourceFile { get; set; }

		[JsonPropertyName("frame_interval")]
		public int FrameInterval { get; set; }

		[JsonPropertyName("frames")]
		public List<string> Frames { get; set; }
	}
}
