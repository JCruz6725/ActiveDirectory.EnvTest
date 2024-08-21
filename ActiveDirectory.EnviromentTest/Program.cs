using System.DirectoryServices.AccountManagement;

namespace ActiveDirectory.EnviromentTest {


    public struct ActiveDirectoryUser {
        public ActiveDirectoryUser(string username, string password) {
            Username = username;
            Password = password;
        }

        public string Username {  get; set; }
        public string Password {  get; set; } 
    }

    internal class Program {
        static void Main(string[] args) {

            Random RandomGenerator = new Random();

            string FullyQualifiedDomainName  = string.Empty;  // Follow the document to see how came up with this
            string DomainPath = string.Empty;                 // This is the top level of the newly created directory 
            
            /*  
             *  Enter the administrator credentials here or create a user and move them to the administrator group
             *  either one will work. Copy and paste their credential here
             */
            ActiveDirectoryUser existingAdminUser = new(username: string.Empty,
                                                        password: string.Empty);


            /*
             *  If these are not set kill program!
             */

            if(string.IsNullOrWhiteSpace(existingAdminUser.Username))
                throw new ArgumentException(nameof(existingAdminUser.Username));

            if(string.IsNullOrWhiteSpace(existingAdminUser.Password))
                throw new ArgumentException(nameof(existingAdminUser.Password));

            if(string.IsNullOrWhiteSpace(FullyQualifiedDomainName))
                throw new ArgumentException(nameof(FullyQualifiedDomainName));

            if(string.IsNullOrWhiteSpace(DomainPath))
                throw new ArgumentException(nameof(DomainPath));



            PrincipalContext context = new(contextType: ContextType.Domain,
                                           name:        FullyQualifiedDomainName,
                                           container:   DomainPath,
                                           userName:    existingAdminUser.Username,
                                           password:    existingAdminUser.Password); // Establish a connection to newly create AD as this user


            // Create user and assign them to a group.
            int RandomInt = RandomGenerator.Next(100000, 999999);
            ActiveDirectoryUser newUser = new ActiveDirectoryUser($"TestUser{RandomInt}", "Password123"); 
            using UserPrincipal userPrincipal = new UserPrincipal(context: context,
                                                                  samAccountName: newUser.Username,
                                                                  password: newUser.Password,
                                                                  enabled: true);
            userPrincipal.Save();
                                    
            // Wait to allow the DC to propigate user.. 
            Task.Delay(2000).Wait();  

            // Create Group
            int OtherRandomInt = RandomGenerator.Next(100000, 999999);  
            using GroupPrincipal groupPrincipal = new GroupPrincipal(context: context,
                                                                     samAccountName: $"TestGroup{OtherRandomInt}");
            groupPrincipal.Members.Add(userPrincipal); // Add the user we created in to the new group
            groupPrincipal.Save();
        }
    }
}
