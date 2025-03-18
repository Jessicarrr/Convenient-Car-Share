using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientCarShare.Data
{
    public class SeedMessageData
    {
        private const string MessageReceiverEmail = "MessageReceiver@test.local";
        private const string MessageSenderEmail = "MessageSender@test.local";
        private const string TestUserPassword = "NotSecure123!!";

        public static async Task InitializeAsync(
            IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            var UserManager = services
                .GetRequiredService<UserManager<ApplicationUser>>();

            await EnsureUserAsync(UserManager, MessageReceiverEmail, TestUserPassword);
            await EnsureUserAsync(UserManager, MessageSenderEmail, TestUserPassword);
            await EnsureMessageAsync(context, UserManager);
        }

        private static async Task EnsureUserAsync(UserManager<ApplicationUser> UserManager,
            string email, string password)
        {
            ApplicationUser TestUser = null;

            try
            {
                TestUser = await UserManager.Users
                   .Where(x => x.UserName == email)
                   .SingleOrDefaultAsync();

                if (TestUser != null) return;

                TestUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                try
                {
                    Task<IdentityResult> taskResult = UserManager.CreateAsync(
                        TestUser, password);
                    taskResult.Wait();
                    var result = taskResult.Result;

                    if (result.Succeeded)
                    {
                        await UserManager.AddToRoleAsync(
                            TestUser, Constants.CustomerRole);
                    }
                }
                catch (Exception e)
                {
                    // this exception is called when there is a db error.
                    // can be caused by adding a new attribute to the user
                    // without editing the above new ApplicationUser
                    string msg = e.Message;
                }
            }
            catch (Exception e2)
            {
                // can be caused by .SingleOrDefaultAsync returning more than one value.
                string msg2 = e2.Message;
            }
        }

        private static async Task EnsureMessageAsync(ApplicationDbContext context,
            UserManager<ApplicationUser> UserManager)
        {
            try
            {
                var MessageReceiverUser = await UserManager.Users
                        .Where(x => x.UserName == MessageReceiverEmail)
                        .SingleOrDefaultAsync();
                var MessageSenderUser = await UserManager.Users
                        .Where(x => x.UserName == MessageSenderEmail)
                        .SingleOrDefaultAsync();

                if (MessageSenderUser == null || MessageReceiverUser == null) return;

                Message message1 = new Message
                {
                    Title = "Woohoooo",
                    Text = "This is a new message. Hello!",
                    Sender = MessageSenderUser,
                    Receiver = MessageReceiverUser,
                    SentDateTime = DateTime.Now
                };

                Message message2 = new Message
                {
                    Title = "Wowee!!",
                    Text = "Another message! Wowee!!",
                    Sender = MessageSenderUser,
                    Receiver = MessageReceiverUser,
                    SentDateTime = DateTime.Now
                };

                Message message3 = new Message
                {
                    Title = "Re: Wowee!!",
                    Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut eget tortor rhoncus," +
                    " tempus sapien ac, consectetur sem. Morbi volutpat, arcu lobortis egestas vestibulum, ex" +
                    " ante porttitor enim, eu cursus odio eros non ante. Fusce quis gravida velit. Sed mollis" +
                    " ullamcorper ante, id mollis ligula ullamcorper ut. Proin et placerat sapien. Nunc non" +
                    " pellentesque tellus, vel rutrum justo. Fusce a quam posuere, bibendum orci ut, faucibus" +
                    " justo. Suspendisse elementum vel tortor non pretium.Duis dapibus consectetur ante, id" +
                    " auctor risus mollis vitae.Proin luctus diam nunc, eget rutrum quam auctor eu.Maecenas" +
                    " maximus elementum mi pulvinar vulputate.Quisque eget suscipit risus.",
                    Sender = MessageSenderUser,
                    Receiver = MessageReceiverUser,
                    SentDateTime = DateTime.Now,
                    ReplyTo = message2
                };

                if (!context.Messages.Where(msg => msg.Receiver.Id == MessageReceiverUser.Id).Any())
                {
                    context.Messages.Add(message1);
                    context.Messages.Add(message2);
                    context.Messages.Add(message3);
                    context.SaveChanges();
                }
            }
            catch(Exception e)
            {
                string thing = e.Message;
                var thing2 = e.InnerException;
                var thing3 = e.InnerException.ToString();
            }
        }
    }
}
