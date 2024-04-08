using System;

namespace LegacyApp
{
    public class UserService
    {
        private IClientRepository clientRepository;

        public UserService()
        {
            clientRepository = new ClientRepository();
        }
        public UserService(IClientRepository clientRepository)
        {
            this.clientRepository = clientRepository;
        }

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!ValidateUserInformation(firstName, lastName, email, dateOfBirth)) return false;
            
            var client = clientRepository.GetById(clientId);

            var user = getUser(firstName, lastName, email, dateOfBirth, client);

            setCardLimitForUser(client, user);

            if (!CheckCardLimit(user)) return false;

            UserDataAccess.AddUser(user);
            return true;
        }

        private static bool CheckCardLimit(User user)
        {
            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            return true;
        }

        private static void setCardLimitForUser(Client client, User user)
        {
            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else if (client.Type == "ImportantClient")
            {
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    creditLimit = creditLimit * 2;
                    user.CreditLimit = creditLimit;
                }
            }
            else
            {
                user.HasCreditLimit = true;
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    user.CreditLimit = creditLimit;
                }
            }
        }

        private static User getUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
            return user;
        }

        private static bool ValidateUserInformation(string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            if (!ValidateName(firstName, lastName)) return false;

            if (!ValidateEmail(email)) return false;

            if (!ValidateDateOfBirth(dateOfBirth)) return false;
            return true;
        }

        private static bool ValidateDateOfBirth(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            if (age < 21)
            {
                return false;
            }

            return true;
        }

        private static bool ValidateEmail(string email)
        {
            if (!email.Contains("@") && !email.Contains("."))
            {
                return false;
            }

            return true;
        }

        private static bool ValidateName(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }

            return true;
        }
    }
}
