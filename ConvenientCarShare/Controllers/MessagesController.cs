using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ConvenientCarShare.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using ConvenientCarShare.Views.Messages;
using ConvenientCarShare.Data;

namespace ConvenientCarShare.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<ApplicationUser> UserManager)
        {
            _context = context;
            _userManager = UserManager;
        }

        // GET: Messages
        
        public ActionResult Compose()
        {
            ComposeModel composeModel = new ComposeModel()
            {

            };

            return View(composeModel);
        }

        public ActionResult ComposeTo(string email, string title)
        {
            ComposeModel composeModel = new ComposeModel()
            {
                Receiver = email,
                Title = title
            };

            return View("Compose", composeModel);
        }

        [HttpPost]
        public async Task<ActionResult> SendMessage(string ReceiverType, string Receiver, string Title, string Text)
        {
            ComposeModel composeModel = new ComposeModel();
            ApplicationUser ReceiverUser = null;

            if(ReceiverType == null)
            {
                composeModel.Errors.Add("Please select a receiver type (broadcast or single user).");
            }

            if(ReceiverType == Constants.MessageSingleUser)
            {
                if (String.IsNullOrEmpty(Receiver))
                {
                    composeModel.Errors.Add("Please enter an email address.");
                }
                else
                {
                    ReceiverUser = await _userManager.FindByEmailAsync(Receiver);

                    if (ReceiverUser == null)
                    {
                        composeModel.Errors.Add("The account associated with that email address could not be found.");
                    }
                }
            }
            
            if (String.IsNullOrEmpty(Title))
            {
                composeModel.Errors.Add("The title field was empty.");
            }
            if (String.IsNullOrEmpty(Text))
            {
                composeModel.Errors.Add("The text message field was empty.");
            }
            if (composeModel.Errors.Count > 0)
            {
                composeModel.Title = Title;
                composeModel.Receiver = Receiver;
                composeModel.Text = Text;
                return View("Compose", composeModel);
            }

            ApplicationUser CurrentUser = await _userManager.GetUserAsync(User);

            if(ReceiverType == Constants.MessageSingleUser)
            {
                Message message = new Message()
                {
                    Receiver = ReceiverUser,
                    Sender = CurrentUser,
                    Title = Title,
                    Text = Text,
                    SentDateTime = DateTime.Now,
                    IsBroadcastMessage = false
                };
                if (Title == "Warning")
                {
                    ReceiverUser.Warnned = true;
                    _context.Update(ReceiverUser);
                        
                }


                _context.Messages.Add(message);
                _context.SaveChanges();

                IndexModel indexModel = await GetIndexModel();
                indexModel.PageMessage = "Successfully sent a message to user \"" + ReceiverUser.Email + "\"";
                return View("Index", indexModel);
            }
            else
            {
                Message message = new Message()
                {
                    Receiver = CurrentUser,
                    Sender = CurrentUser,
                    Title = Title,
                    Text = Text,
                    SentDateTime = DateTime.Now,
                    IsBroadcastMessage = true
                };
                _context.Messages.Add(message);
                _context.SaveChanges();

                IndexModel indexModel = await GetIndexModel();
                indexModel.PageMessage = "Successfully broadcasted a message to everybody.";
                return View("Index", indexModel);
            }
            
        }

        [HttpPost]
        public async Task<ActionResult> SendReply(string Title, string Text, int ReplyingTo)
        {
            var replyingToMessage = await _context.Messages
                .Include(message => message.Sender)
                .Where(message => message.ID == ReplyingTo)
                .SingleOrDefaultAsync();

            ApplicationUser CurrentUser = await _userManager.GetUserAsync(User);

            ReplyModel replyModel = new ReplyModel()
            {
                Title = Title,
                Text = Text
            };

            if (replyingToMessage == null)
            {
                IndexModel indexModel = await GetIndexModel();
                indexModel.Errors.Add("An unknown error occurred when attempting to reply to a message.");
                return View("Index", indexModel);
            }
            else if (replyingToMessage.Receiver != CurrentUser)
            {
                IndexModel indexModel = await GetIndexModel();
                indexModel.Errors.Add("An unknown error occurred when attempting to reply to a message.");
                return View("Index", indexModel);
            }

            replyModel.Receiver = replyingToMessage.Receiver.Email;
            replyModel.ReplyingTo = ReplyingTo;

            if(String.IsNullOrEmpty(Title))
            {
                replyModel.Errors.Add("The title field was empty.");
            }
            if(String.IsNullOrEmpty(Text))
            {
                replyModel.Errors.Add("The text message field was empty.");
            }

            if(replyModel.Errors.Any())
            {
                return View("Reply", replyModel);
            }

            Message newMessage = new Message()
            {
                ReplyTo = replyingToMessage,
                Title = Title,
                Text = Text,
                Sender = CurrentUser,
                Receiver = replyingToMessage.Sender,
                SentDateTime = DateTime.Now
            };

            _context.Messages.Add(newMessage);
            _context.SaveChanges();

            var newIndexModel = await GetIndexModel();
            newIndexModel.PageMessage = "Successfully replied to \"" + replyingToMessage.Title + "\" by " + replyingToMessage.Sender.Email;
            return View("Index", newIndexModel);
        }
        public async Task<ActionResult> Index()
        {
            IndexModel model = await GetIndexModel();

            return View(model);
        }

        public bool MarkMessageAsRead(int Id)
        {
            var message = _context.Messages.Find(Id);

            if(message == null)
            {
                return false;
            }

            if(message.HasBeenRead)
            {
                return false;
            }

            message.HasBeenRead = true;
            _context.SaveChanges();
            return true;
        }

        public async Task<ActionResult> Reply(int ReplyingTo)
        {
            var replyingToMessage = await _context.Messages
                .Include(message => message.Sender)
                .Where(message => message.ID == ReplyingTo)
                .SingleAsync();

            ApplicationUser CurrentUser = await _userManager.GetUserAsync(User);

            if (replyingToMessage == null)
            {
                IndexModel model = await GetIndexModel();
                model.Errors.Add("An unknown error occurred when attempting to reply to a message.");
                return View("Index", model);
            }
            else if (replyingToMessage.Receiver != CurrentUser)
            {
                IndexModel model = await GetIndexModel();
                model.Errors.Add("An unknown error occurred when attempting to reply to a message.");
                return View("Index", model);
            }

            var newMessageTitle = replyingToMessage.Title.StartsWith("re: ") ? replyingToMessage.Title : "re: " + replyingToMessage.Title;

            var replyModel = new ReplyModel()
            {
                Receiver = replyingToMessage.Sender.Email,
                Title = newMessageTitle,
                ReplyingTo = ReplyingTo
            };

            return View(replyModel);
        }

        public async Task<int> GetNumUnreadMessages()
        {
            ApplicationUser CurrentUser = await _userManager.GetUserAsync(User);

            var userMessages = _context.Messages
            .Where(message => message.Receiver == CurrentUser)
            .Where(message => message.HasBeenRead == false)
            .ToList();

            return userMessages.Count();

        }

        private void GetAllChildMessages(Message message, List<Message> messageList, ref List<Message> messageTree)
        {
            var childMessages = messageList.Where(m => m.ReplyTo == message).ToArray();

            if(childMessages.Any())
            {
                foreach(var childMessage in childMessages)
                {
                    messageTree.Add(childMessage);
                    GetAllChildMessages(childMessage, messageList, ref messageTree);
                }
            }
        }

        private async Task<IndexModel> GetIndexModel()
        {
            //current user
            ApplicationUser CurrentUser = await _userManager.GetUserAsync(User);

            bool IsAdmin = false;

            if (User.IsInRole(Constants.AdministratorRole))
            {
                IsAdmin = true;
            }

            var allUserMessages = await _context.Messages
                .Include(message => message.Sender)
                .Include(x => x.Receiver)
                .Where(message => message.Receiver.Id == CurrentUser.Id
                || message.Sender.Id == CurrentUser.Id
                || message.IsBroadcastMessage == true)
                .OrderBy(message => message.SentDateTime)
                .ToListAsync();

            var parentMessages = allUserMessages
                .Where(message => message.ReplyTo == null)
                .ToList();

            var messageTrees = new List<List<Message>>();
            var messageTree = new List<Message>();

            foreach (var parentMessage in parentMessages)
            {
                messageTree.Add(parentMessage);
                GetAllChildMessages(parentMessage, allUserMessages, ref messageTree);
                var messageTreeCopy = new List<Message>();
                messageTreeCopy.AddRange(messageTree);
                messageTrees.Add(messageTreeCopy);
                messageTree.Clear();
            }

            var model = new IndexModel()
            {
                MessagesAndReplies = messageTrees,
                IsAdmin = IsAdmin,
                CurrentUser = CurrentUser
            };

            return model;
        }

        /*// GET: Messages/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Messages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Messages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Messages/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Messages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Messages/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Messages/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }*/
    }
}