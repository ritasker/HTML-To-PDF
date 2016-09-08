using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfGen
{
    using System.IO;
    using HandlebarsDotNet;
    using IronPdf;
    using Microsoft.Owin.Hosting;
    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.ModelBinding;
    using Nancy.Owin;
    using Owin;

    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://+:6969"))
                Console.ReadLine();
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            StaticConfiguration.DisableErrorTraces = false;
            appBuilder.UseNancy(new NancyOptions()
            {
                Bootstrapper = new Bootstrapper()
            });
        }
    }

    internal class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override IRootPathProvider RootPathProvider
        {
            get
            {
                return new RootProvider();
            }
        }
    }

    public class ByteArrayResponse : Response
    {
        public ByteArrayResponse(byte[] body, string contentType = null)
        {
            ContentType = contentType ?? "application/octet-stream";
            Contents = stream =>
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream))
                    binaryWriter.Write(body);
            };
        }
    }

    public class GeneratorModule : NancyModule
    {
        public GeneratorModule()
        {
            StaticConfiguration.DisableErrorTraces = false;
            Get["/"] = _ => View["Index"];
            Post["/generate"] = p =>
           {
               HtmlInput htmlInput = this.Bind<HtmlInput>();
               byte[] binaryData;
               try
               {
                   var options = new HtmlToPdf()
                   {
                       PrintOptions = new PdfPrintOptions()
                       {
                           MarginBottom = 0,
                           MarginLeft = 0,
                           MarginRight = 0,
                           MarginTop = 0,
                           EnableJavaScript = true
                       }
                   };

                   var tickets = new MegaTicket
                   {
                       Tickets =
                           Enumerable.Range(0, htmlInput.Times)
                               .Select(t => new Ticket()
                               {
                                   Name = "Name " + t
                               }).ToArray()
                   };

                   var template = Handlebars.Compile(htmlInput.Body);

                   var document = template(tickets);

                   var result = options.RenderHtmlAsPdf(document);

                   binaryData = result.BinaryData;
               }
               catch (Exception ex)
               {
                   return ex.Message;
               }
               return new ByteArrayResponse(binaryData, "application/pdf");
           };
        }
    }

    internal class HtmlInput
    {
        public string Body { get; set; }

        public int Times { get; set; }
    }

    internal class MegaTicket
    {
        public Ticket[] Tickets { get; set; }
    }

    internal class RootProvider : IRootPathProvider, IHideObjectMembers
    {
        public string GetRootPath()
        {
            return "C:\\Temp";
        }

        Type IHideObjectMembers.GetType()
        {
            return GetType();
        }
    }

    internal class Ticket
    {
        public string Name { get; set; }
    }
}
