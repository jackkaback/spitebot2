using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace spiteBot2
{
	class Program
	{
		public static Dictionary<char, Func<string, string, int>> actions = new Dictionary<char, Func<string, string, int>>();
		public static Dictionary<string, user> users = new Dictionary<string, user>();

		public static user owner = new user("drcobaltjedi", true, true);

		public static char free = '*';
		public static char userChar = '!';
		public static char admin = '&';
		

		static void Main(string[] args)
		{
			users.Add(owner.Name, owner);
			Console.WriteLine("Hello World!");
		}

		#region objects and functions
		#region Users
		public class user
		{
			private bool isAdmin = false;
			private bool isApprovedUser = false;
			private string name;

			public user(string name)
			{
				this.name = name;
			}
			public user(string name, bool isApprovedUser, bool isAdmin)
			{
				this.name = name;
				this.isApprovedUser = isApprovedUser;
				this.isAdmin = isAdmin;
			}
			public bool IsAdmin
			{
				get => isAdmin;
				set => isAdmin = value;
			}
			public bool IsApprovedUser
			{
				get => isApprovedUser;
				set => isApprovedUser = value;
			}
			public string Name
			{
				get => name;
				set => name = value;
			}
		}
		#endregion

		public static int freeBes(string acct, string msg)
		{


			return 1;
		}
		#endregion
	}
}