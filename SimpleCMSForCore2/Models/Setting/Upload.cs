using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleCMSForCore2.Models.Setting
{
    public class Upload
    {
        public string AllowImageFileType { get; set; }
        public string AllowAudioFileType { get; set; }
        public string AllowVideoFileType { get; set; }
        public  int AllowUploadSize { get; set; }

    }
}
