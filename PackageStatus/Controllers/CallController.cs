using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PackageStatus.Models;
using Twilio;
using Twilio.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Mvc;

namespace PackageStatus.Controllers
{
    public class CallController : TwilioController
    {
        public ActionResult Index(VoiceRequest request)
        {
            var response = new TwilioResponse();
            response.Say("Thank you for calling the TPS package status hotline.");
            response.Redirect( Url.Action("Gather"), "GET" );
            
            return TwiML(response);
        }
        
        public ActionResult Gather()
        {
            var response = new TwilioResponse();
            response.BeginGather(new { action=Url.Action("Gather"), method="POST" })
                .Say("Please enter your four digit package ID, followed by the pound sign.");
            response.EndGather();

            response.Say("I'm sorry, I didn't get that.");
            response.Redirect( Url.Action("Gather"), "GET" );

            return TwiML(response);
        }

        [HttpPost]
        public ActionResult Gather(VoiceRequest request)
        {
            var response = new TwilioResponse();
            response.Say("Got it.  Wait one moment while I locate your package status.");
            response.Redirect( Url.Action("Lookup", new { csid=request.CallSid, pid=request.Digits }), "GET" );
            return TwiML(response);
        }

        public ActionResult Lookup(string callsid, string pid)
        {
            var packageService = new PackageService();

            Task<Package>.Factory.StartNew(
                p => packageService.LocatePackage(pid),
                new Dictionary<string, string>() { 
                    { "CallSid", callsid },
                    { "FoundRedirectUrl", Url.Action("Complete", null, null, Request.Url.Scheme) },
                    { "NotFoundRedirectUrl", Url.Action("NotFound", null, null, Request.Url.Scheme) },
                    { "ErrorRedirectUrl", Url.Action("Error", null, null, Request.Url.Scheme) }
                },
                CancellationToken.None
            ).ContinueWith(t =>
                {
                    Dictionary<string, string> state = (Dictionary<string, string>)t.AsyncState;

                    if (t.IsCanceled || t.IsFaulted)
                    {
                        RedirectCall(state["CallSid"], state["ErrorRedirectUrl"]);
                    }

                    if (t.Result == null)
                    {
                        RedirectCall(state["CallSid"], state["NotFoundRedirectUrl"]);
                    }
                    else
                    {
                        string url = string.Format("{0}?s={1}", state["FoundRedirectUrl"], t.Result.Status);
                        RedirectCall(state["CallSid"], url);
                    }

                    return;
                }
            );

            var response = new TwilioResponse();
            response.Play(
                Url.AbsoluteContent("~/Content/Audio/clocktickfast.mp3", Request.Url.AbsoluteUri),
                new { loop = "true" });
            return TwiML(response);
        }

        private void RedirectCall(string callSid, string url)
        {
            string accountSid = "[YOUR_ACCOUNT_SID]";
            string authToken = "[YOUR_AUTH_TOKEN]";

            var client = new TwilioRestClient(accountSid, authToken);
            client.RedirectCall(callSid, url, "GET");
        }

        public ActionResult Error()
        {
            var response = new TwilioResponse();
            response.Say(string.Format("I'm sorry, there seems to have been a problem locating your package."));

            response.BeginGather(new { action = Url.Action("Complete"), method = "POST" })
                .Say("To speak with an agent, press one.");
            response.EndGather();

            response.Redirect(Url.Action("Goodbye"), "GET");
            return TwiML(response);
        }

        [HttpPost]
        public ActionResult Error(VoiceRequest request)
        {
            var response = new TwilioResponse();
            switch (request.Digits)
            {
                case "1":
                    response.Redirect(Url.Action("Agent"), "GET");
                    break;
                default:
                    response.Redirect(Url.Action("Goodbye"), "GET");
                    break;
            }

            return TwiML(response);
        }

        public ActionResult NotFound()
        {
            var response = new TwilioResponse();
            response.Say(string.Format("I'm sorry, I could not locate your package."));

            response.BeginGather(new { action = Url.Action("Complete"), method = "POST" })
                .Say("To try to locate the package again or to locate another package, press one.")
                .Say("To speak with an agent, press two.");
            response.EndGather();

            response.Redirect(Url.Action("Goodbye"), "GET");
            return TwiML(response);
        }

        [HttpPost]
        public ActionResult NotFound(VoiceRequest request)
        {
            var response = new TwilioResponse();
            switch (request.Digits)
            {
                case "1" :
                    response.Redirect(Url.Action("Gather"), "GET");
                    break;
                case "2":
                    response.Redirect(Url.Action("Agent"), "GET");
                    break;
                default:
                    response.Redirect(Url.Action("Goodbye"), "GET");
                    break;
            }

            return TwiML(response);
        }

        public ActionResult Complete(string s)
        {
            var response = new TwilioResponse();
            response.Say(string.Format("The status of your package is {0}.", s));

            response.BeginGather(new { action = Url.Action("Complete"), method="POST" })
                .Say("To locate another package, press one.");
            response.EndGather();

            response.Redirect( Url.Action("Goodbye"), "GET" );
            return TwiML(response);
        }

        [HttpPost]
        public ActionResult Complete(VoiceRequest request)
        {
            var response = new TwilioResponse();
            if (request.Digits=="1")
            {
                response.Redirect( Url.Action("Gather"), "GET" );
            }
            else
            {
                response.Redirect( Url.Action("Goodbye"), "GET" );   
            }
            return TwiML(response);
        }

        public ActionResult Goodbye()
        {
            var response = new TwilioResponse();
            response.Say("Goodbye");
            response.Hangup();

            return TwiML(response);
        }
    }
}
