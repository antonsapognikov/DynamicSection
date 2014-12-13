using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using San.Web.DynamicSection.Context;

namespace San.Web.DynamicSection.Filters
{
    public class SectionContextFilter : Stream
    {
        private readonly Stream stream;

        public SectionContextFilter(Stream stream)
        {
            this.stream = stream;
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new InvalidOperationException(); }
        }

        public override long Position
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            List<Guid> rendered = SectionContext.GetRendered();
            if (rendered.Any())
            {
                Dictionary<Guid, int> sectionIndexes = SectionContextUtils.FindUsages(buffer, offset, count);
                if (sectionIndexes.Any())
                {
                    foreach (Guid sectionId in rendered)
                    {
                        if (!sectionIndexes.ContainsKey(sectionId))
                            continue;
                        int index = sectionIndexes[sectionId];
                        IHtmlString html = SectionContext.GetHtml(sectionId) ?? new HtmlString(string.Empty);
                        byte[] injectBytes = Encoding.UTF8.GetBytes(html.ToHtmlString());
                        stream.Write(buffer, offset, index - offset);
                        stream.Write(injectBytes, 0, injectBytes.Length);
                        count -= index - offset + SectionContextUtils.BytesCount;
                        offset = index + SectionContextUtils.BytesCount;
                        SectionContext.Remove(sectionId);
                    }
                }              
            }
            stream.Write(buffer, offset, count);
        }
    }
}