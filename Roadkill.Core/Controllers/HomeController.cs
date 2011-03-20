﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using System.Web.Management;
using System.Data.SqlClient;
using Roadkill.Core.Converters;
using Roadkill.Core.Search;

namespace Roadkill.Core.Controllers
{
	public class HomeController : ControllerBase
    {
		public ActionResult Index()
		{
			PageManager manager = new PageManager();
			PageSummary summary = manager.FindByTag("homepage").FirstOrDefault();
			if (summary == null)
			{
				summary = new PageSummary();
				summary.Title = "You have no mainpage set";
				summary.Content = "To set a main page, create a page and assign the tag 'homepage' to it.";
				summary.CreatedBy = "";
				summary.CreatedOn = DateTime.Now;
				summary.Tags = "homepage";
				summary.ModifiedOn = DateTime.Now;
				summary.ModifiedBy = "";
			}

			RoadkillContext.Current.Page = summary;

			return View(summary);
		}

		public ActionResult Login()
		{
			if (Request.QueryString["ReturnUrl"] != null && Request.QueryString["ReturnUrl"].Contains("Files"))
				return View("BlankLogin");

			return View();
		}

		public ActionResult GlobalJsVars()
		{
			UrlHelper helper = new UrlHelper(HttpContext.Request.RequestContext);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine(string.Format("var ROADKILL_CORESCRIPTPATH = '{0}';", helper.Content("~/Assets/Scripts/")));

			if (RoadkillContext.Current.IsLoggedIn)
			{
				builder.AppendLine(string.Format("var ROADKILL_WIKIMARKUPHELP = '{0}';", helper.Action(RoadkillSettings.MarkupType + "Reference", "Help")));
				builder.AppendLine(string.Format("var ROADKILL_THEMEPATH =  '{0}/';", Url.Content(RoadkillSettings.ThemePath)));

				// Edit page variables
				builder.AppendLine(string.Format("var ROADKILL_TAGAJAXURL = '{0}/';", helper.Action("AllTags", "Pages")));
				builder.AppendLine(string.Format("var ROADKILL_PREVIEWURL = '{0}/';", helper.Action("GetPreview", "Pages")));
				builder.AppendLine(string.Format("var ROADKILL_MARKUPTYPE = '{0}';", RoadkillSettings.MarkupType));

				// File manager variables
				builder.AppendLine(string.Format("var ROADKILL_FILEMANAGERURL = '{0}';", helper.Action("Index", "Files")));
				builder.AppendLine(string.Format("var ROADKILL_FILETREE_URL =  '{0}/';", Url.Action("Folder","Files")));
				builder.AppendLine(string.Format("var ROADKILL_FILETREE_PATHNAME_URL =  '{0}/';", Url.Action("GetPath", "Files")));
				builder.AppendLine(string.Format("var ROADKILL_ATTACHMENTSPATH = '{0}';", Url.Content(RoadkillSettings.AttachmentsFolder)));

				// Tokens for the edit toolbar
				MarkupConverter converter = new MarkupConverter();
				IParser parser = converter.GetParser();
				builder.AppendLine(string.Format("var ROADKILL_EDIT_BOLD_TOKEN = \"{0}\";", parser.BoldToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_ITALIC_TOKEN = \"{0}\";", parser.ItalicToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_UNDERLINE_TOKEN = \"{0}\";", parser.UnderlineToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_LINK_STARTTOKEN = \"{0}\";", parser.LinkStartToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_LINK_ENDTOKEN = \"{0}\";", parser.LinkEndToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_IMAGE_STARTTOKEN = \"{0}\";", parser.ImageStartToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_IMAGE_ENDTOKEN = \"{0}\";", parser.ImageEndToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_NUMBERLIST_TOKEN = \"{0}\";", parser.NumberedListToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_BULLETLIST_TOKEN = \"{0}\";", parser.BulletListToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_HEADING_TOKEN = \"{0}\";", parser.HeadingToken));
			}

			return Content(builder.ToString(), "text/javascript");
		}

		[HttpPost]
		public ActionResult Login(string username, string password, string fromUrl)
		{
			UserManager manager = new UserManager();
			if (manager.Authenticate(username, password))
			{
				FormsAuthentication.SetAuthCookie(username, true);

				if (!string.IsNullOrWhiteSpace(fromUrl))
					return Redirect(fromUrl);
				else
					return RedirectToAction("Index");
			}
			else
			{
				ModelState.AddModelError("Username/Password", "The username/password are incorrect");
				return View();
			}
		}

		public ActionResult Logout()
		{
			UserManager manager = new UserManager();
			manager.Logout();
			return RedirectToAction("Index");
		}

		public ActionResult Search(string q)
		{
			ViewData["search"] = q;

			List<SearchResult> results = SearchManager.SearchIndex(q);
			return View(results);
		}
    }
}