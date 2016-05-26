using MvcWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Text;
using System.Security.Principal;
using System.Web.Security;
using System.Web;
using System.Threading;
using System.Web.Http.Metadata;
using Newtonsoft.Json.Linq;
using System.Web.Http.WebHost;
using System.Web.SessionState;
using System.Web.Routing;

namespace MvcWebApi.Controllers
{

    public class CustomParameterBinding : HttpParameterBinding
    {
        public CustomParameterBinding(HttpParameterDescriptor descriptor)
            : base(descriptor)
        {

        }


        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider,
                                                    HttpActionContext actionContext,
                                                    CancellationToken cancellationToken)
        {
            var binding = actionContext
                .ActionDescriptor
                .ActionBinding;

            if (binding.ParameterBindings.Length > 1 ||
                actionContext.Request.Method == HttpMethod.Get)
                return EmptyTask.Start();

            var type = binding
                        .ParameterBindings[0]
                        .Descriptor.ParameterType;

            if (type == typeof(string))
            {
                return actionContext.Request.Content
                        .ReadAsStringAsync()
                        .ContinueWith((task) =>
                        {
                            var stringResult = task.Result;
                            SetValue(actionContext, stringResult);
                        });
            }
            else if (type == typeof(byte[]))
            {
                return actionContext.Request.Content
                    .ReadAsByteArrayAsync()
                    .ContinueWith((task) =>
                    {
                        byte[] result = task.Result;
                        SetValue(actionContext, result);
                    });
            }

            throw new InvalidOperationException("Only string and byte[] are supported for [NakedBody] parameters");
        }

        public override bool WillReadBody
        {
            get
            {
                return true;
            }
        }
    }
    public class BasicAuthenticationIdentity : GenericIdentity
    {
        public string Password { get; set; }
        public BasicAuthenticationIdentity(string name, string password)
            : base(name, "Basic")
        {
            this.Password = password;
        }
    }

    public class EmptyTask
    {
        public static Task Start()
        {
            var taskSource = new TaskCompletionSource<AsyncVoid>();
            taskSource.SetResult(default(AsyncVoid));
            return taskSource.Task as Task;
        }

        private struct AsyncVoid
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class CustomBodyAttribute : ParameterBindingAttribute
    {
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            if (parameter == null)
                throw new ArgumentException("Invalid parameter");

            return new CustomParameterBinding(parameter);
        }
    }

    public class BasicAuthenticationHandler : DelegatingHandler
    {
        private const string authenticationHeader = "WWW-Authenticate";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var crendentials = ParseHeader(request);

            if (crendentials != null)
            {
                var identity = new BasicAuthenticationIdentity(crendentials.Name, crendentials.Password);

                var principal = new GenericPrincipal(identity, null);

                Thread.CurrentPrincipal = principal;

                //针对于ASP.NET设置
                //if (HttpContext.Current != null)
                //    HttpContext.Current.User = principal;
            }

            return base.SendAsync(request, cancellationToken).ContinueWith(task =>
            {
                var response = task.Result;
                if (crendentials == null && response.StatusCode == HttpStatusCode.Unauthorized)

                    Challenge(request, response);

                return response;
            });



        }

        void Challenge(HttpRequestMessage request, HttpResponseMessage response)
        {
            var host = request.RequestUri.DnsSafeHost;

            response.Headers.Add(authenticationHeader, string.Format("Basic realm=\"{0}\"", host));

        }

        public virtual BasicAuthenticationIdentity ParseHeader(HttpRequestMessage requestMessage)
        {
            string authParameter = null;

            var authValue = requestMessage.Headers.Authorization;
            if (authValue != null && authValue.Scheme == "Basic")
                authParameter = authValue.Parameter;

            if (string.IsNullOrEmpty(authParameter))

                return null;

            authParameter = Encoding.Default.GetString(Convert.FromBase64String(authParameter));

            var authToken = authParameter.Split(':');
            if (authToken.Length < 2)
                return null;

            return new BasicAuthenticationIdentity(authToken[0], authToken[1]);
        }
    }

    public class BasicAuthenticationFilter : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {

            var identity = Thread.CurrentPrincipal.Identity;
            if (identity != null && HttpContext.Current != null)
                identity = HttpContext.Current.User.Identity;

            if (identity != null && identity.IsAuthenticated)
            {

                var basicAuthIdentity = identity as BasicAuthenticationIdentity;

                if (basicAuthIdentity.Name == "xpy0928" && basicAuthIdentity.Password == "cnblogs")
                {
                    return true;
                }
            }

            return false;

        }
    }

    public class User {

        public string Name { get; set; }
        public int Age { get; set; }

        public string Gender { get; set; }
    }
    public class UserRequest
    {

        public User User { get; set; }
        public string UserToken { get; set; }
    }

    public class UserResponse
    {
        public string Result { get; set; }

        public int StatusCode { get; set; }

        public string ErrorMessage { get; set; }
    }


  

    public class ProductController : ApiController
    {
        [HttpPost]
        public string PostUser(JObject jb)
        {
            var a = HttpContext.Current.Session["xpy0928"];
            if ((HttpContext.Current.Session["xpy0928"] as string) == null)
                HttpContext.Current.Session["xpy0928"] = "嗨-博客";
            return (HttpContext.Current.Session["xpy0928"] as string);
            //dynamic json = jb;
            //JObject userJson = json.User;
            //string userToken = json.UserToken;

            //var user = userJson.ToObject<User>();

            //return string.Format("name:{0},age:{1},userToken:{2}", user.Name, user.Age, userToken);

        }


    }
}