using System;
using System.Collections.Generic;

namespace Lab10
{

    public class Alert
    {
        public int Type;

        public string Info;

        public Alert(int type, string info)
        {
            Type = type;
            Info = info;
        }
    }

    public class Decision
    {
        public string Info;

        public Decision(string info)
        {
            Info = info;
        }

        public string DoIt()
        {
            return Info + "... Doing it";
        }
    }

    public interface IHandler
    {
        void PostAlert(Alert alert, string to);

        void HandleAlert(Alert alert);

        bool GetEvacuatedStatus();

    }

    public abstract class AbstractHandler : IHandler
    {

        private string _name;

        private bool _evacuated = false;

        private List<IHandler> _subordinates;

        private IHandler _headPerson;

        public bool GetEvacuatedStatus()
        {
            return _evacuated;
        }

        public void SetEvacuated(bool status)
        {
            _evacuated = status;
        }

        public void PostAlert(Alert alert, string to)
        {
            if (to == "boss")
            {
                GetHeadPerson().HandleAlert(alert);
            }

            else
            {
                foreach (var person in GetSubordinates())
                {
                    person.HandleAlert(alert);
                }
            }
        }

        public void OrderEvacuationOfSubordinates()
        {
            Console.WriteLine(GetName() + ", Ordering evacuation of all subordinates");
            Alert newAlert = new Alert(99, "Evacuate all personnel now!");
            PostAlert(newAlert, "subordinates");
        }

        public void Evacuate()
        {
            if (this is Worker)
            {
                SetEvacuated(true);
                Console.WriteLine(GetName() + " is now evacuated");
            }

            else
            {
                if (SubordinatesEvacuated())
                {
                    SetEvacuated(true);
                    Console.WriteLine(GetName() + " is now evacuated");
                }
            }
        }

        public bool SubordinatesEvacuated()
        {

            foreach (var subordinate in GetSubordinates())
            {
                if (!subordinate.GetEvacuatedStatus())
                {
                    return false;
                }

            }

            return true;
        }

        public abstract void HandleAlert(Alert alert);

        public abstract void SeeDanger();

        public List<IHandler> GetSubordinates()
        {
            return this is Worker ? null : _subordinates;
        }

        public void SetSubordinates(List<IHandler> people)
        {
            if (this is Worker)
            {
                return;
            }

            _subordinates = people;
        }

        public IHandler GetHeadPerson()
        {
            return this is Ceo ? null : _headPerson;
        }

        public void SetHeadPerson(IHandler person)
        {
            if (this is Ceo)
            {
                return;
            }

            _headPerson = person;

        }

        public string GetName()
        {
            return _name;
        }

        public void SetName(string name)
        {
            _name = name;
        }
    }

    public class Ceo : AbstractHandler
    {
        private List<Decision> _decisionOptions = new List<Decision>();
        private static int _decisionSuggestionCounter = 0;

        public override void SeeDanger()
        {
            Console.WriteLine("CEO, " + GetName() + ", Throwing meeting");
            Alert newAlert = new Alert(2, "Request Decisions from Managers");
            PostAlert(newAlert, "subordinates");
        }

        public void Grant()
        {
            if (_decisionSuggestionCounter == 1) // need to randomize this...
            {
                Decision decision = _decisionOptions[0];
                Console.WriteLine("The CEO chose " + decision.DoIt());
            }

            _decisionSuggestionCounter++;
        }

        public override void HandleAlert(Alert alert)
        {
            switch (alert.Type)
            {
                case 1:
                    SeeDanger();
                    break;

                case 3:

                    // sleep for a two seconds first to assure all manager's decisions have been reached 
                    System.Threading.Thread.Sleep(2000);

                    // get Manager Decisions
                    foreach (var manager in GetSubordinates())
                    {
                        _decisionOptions.Add(((Manager)manager).ManagerDecision);
                    }

                    Grant();
                    break;

                case 99:
                    OrderEvacuationOfSubordinates();
                    if (SubordinatesEvacuated())
                    {
                        Evacuate();
                    }
                    break;

                default:
                    return;
            }

        }
    }

    public class Manager : AbstractHandler
    {

        public Decision ManagerDecision;

        public override void SeeDanger()
        {
            Console.WriteLine("Manager, " + GetName() + ", requesting info from supervisors");
            Alert alert = new Alert(4, "Need info from supervisors");
            PostAlert(alert, "subordinates");

        }

        public void ContactBoss(Alert alert)
        {
            PostAlert(alert, "boss");
        }

        public Decision SuggestDecision()
        {
            Console.WriteLine("Manager, " + GetName() + ", suggesting decision.");
            Decision decision = new Decision("Manager, " + GetName() + "'s decision " +
                                             "that the city’s environmental department is to be notified");
            ManagerDecision = decision;
            Alert alert = new Alert(3, "Decision Reached");
            ContactBoss(alert);

            return decision;
        }

        public override void HandleAlert(Alert alert)
        {
            switch (alert.Type)
            {
                case 1:
                    SeeDanger();
                    ContactBoss(alert);
                    break;
                case 2:
                    SuggestDecision();
                    break;
                case 99:
                    OrderEvacuationOfSubordinates();
                    if (SubordinatesEvacuated())
                    {
                        Evacuate();
                    }

                    break;
                default:
                    return;
            }
        }
    }

    public class ProjectLeader : AbstractHandler
    {

        public override void SeeDanger()
        {
            Console.WriteLine("Project Leader, " + GetName() + ", requesting workers to fix it.");
            Alert alert = new Alert(5, "Fix the problem");
            PostAlert(alert, "subordinates");
        }

        public string ProvideInfo()
        {
            string output = "Information from Project Leader, " + GetName();
            Console.WriteLine(output);

            return output;
        }

        public override void HandleAlert(Alert alert)
        {
            switch (alert.Type)
            {
                case 1:
                    PostAlert(alert, "subordinates");
                    PostAlert(alert, "boss");
                    break;
                case 4:
                    ProvideInfo();
                    break;
                case 99:
                    OrderEvacuationOfSubordinates();
                    if (SubordinatesEvacuated())
                    {
                        Evacuate();
                    }

                    break;
                default:
                    return;
            }
        }
    }

    public class Supervisor : AbstractHandler
    {

        public override void SeeDanger()
        {
            Console.WriteLine("Supervisor, " + GetName() + ", requesting workers to fix it.");
            Alert alert = new Alert(5, "Fix the problem");
            PostAlert(alert, "subordinates");
        }

        public string ProvideInfo()
        {
            string output = "Information from Supervisor, " + GetName();
            Console.WriteLine(output);

            return output;
        }

        public void SendSupport()
        {
            Worker workerOne, workerTwo;

            if (GetName() == "Jeff")
            {
                workerOne = (Worker)GetSubordinates()[0];
                workerTwo = (Worker)GetSubordinates()[2];
            }
            else
            {
                workerOne = (Worker)GetSubordinates()[0];
                workerTwo = (Worker)GetSubordinates()[1];
            }

            Console.WriteLine("Sending support from the workers: "
                              + workerOne.GetName() + " and " + workerTwo.GetName());
        }

        public override void HandleAlert(Alert alert)
        {
            switch (alert.Type)
            {
                case 1:
                    SeeDanger();
                    PostAlert(alert, "boss");
                    break;
                case 4:
                    ProvideInfo();
                    SendSupport();
                    break;
                case 99:
                    OrderEvacuationOfSubordinates();
                    if (SubordinatesEvacuated())
                    {
                        Evacuate();
                    }

                    break;
                default:
                    return;
            }
        }
    }

    public class Worker : AbstractHandler
    {

        public override void SeeDanger()
        {
            Console.WriteLine("Worker, " + GetName() + ", sees a gas leak in the big tank.");
            Alert alert = new Alert(1, "There is a gas leak in the big tank");
            Console.WriteLine("Worker, " + GetName() + ", alerting boss about the incident.");
            PostAlert(alert, "boss");
        }

        public void FixIt(Alert alert)
        {
            Console.WriteLine("Worker, " + GetName() + ", is fixing: " + alert.Info);
        }

        public override void HandleAlert(Alert alert)
        {
            switch (alert.Type)
            {
                case 5:
                    FixIt(alert);
                    break;
                case 99:
                    Evacuate();
                    break;
                default:
                    return;
            }
        }
    }


    internal class Program
    {
        public static void Main(string[] args)
        {

            // create project leaders
            ProjectLeader Chuck = new ProjectLeader();
            Chuck.SetName("Chuck");

            ProjectLeader Denise = new ProjectLeader();
            Denise.SetName("Denise");

            // create supervisors
            Supervisor Jack = new Supervisor();
            Jack.SetName("Jack");

            Supervisor Jeff = new Supervisor();
            Jeff.SetName("Jeff");

            // create workers 
            Worker John = new Worker();
            John.SetName("John");
            John.SetHeadPerson(Jack);

            Worker Mary = new Worker();
            Mary.SetName("Mary");
            Mary.SetHeadPerson(Jack);

            Worker Jane = new Worker();
            Jane.SetName("Jane");
            Jane.SetHeadPerson(Jack);

            Worker Tom = new Worker();
            Tom.SetName("Tom");
            Tom.SetHeadPerson(Jack);

            Worker Nick = new Worker();
            Nick.SetName("Nick");
            Nick.SetHeadPerson(Jack);

            Worker Rob = new Worker();
            Rob.SetName("Rob");
            Rob.SetHeadPerson(Jeff);

            Worker Ed = new Worker();
            Ed.SetName("Ed");
            Ed.SetHeadPerson(Jeff);

            Worker Rick = new Worker();
            Rick.SetName("Rick");
            Rick.SetHeadPerson(Jeff);

            Worker Michael = new Worker();
            Michael.SetName("Michael");
            Michael.SetHeadPerson(Jeff);

            Worker Joe = new Worker();
            Joe.SetName("Joe");
            Joe.SetHeadPerson(Chuck);

            Worker Frank = new Worker();
            Frank.SetName("Frank");
            Frank.SetHeadPerson(Chuck);

            Worker Sam = new Worker();
            Sam.SetName("Sam");
            Sam.SetHeadPerson(Chuck);

            Worker Greg = new Worker();
            Greg.SetName("Greg");
            Greg.SetHeadPerson(Chuck);

            Worker Amy = new Worker();
            Amy.SetName("Amy");
            Amy.SetHeadPerson(Denise);

            Worker Wil = new Worker();
            Wil.SetName("Wil");
            Wil.SetHeadPerson(Denise);

            Worker Nancy = new Worker();
            Nancy.SetName("Nancy");
            Nancy.SetHeadPerson(Denise);

            Worker Adam = new Worker();
            Adam.SetName("Adam");
            Adam.SetHeadPerson(Denise);

            // create CEO
            Ceo Steve = new Ceo();
            Steve.SetName("Steve");

            // create managers
            Manager Bob = new Manager();
            Bob.SetName("Bob");
            Bob.SetHeadPerson(Steve);
            Bob.SetSubordinates(new List<IHandler> { Jack, Jeff });

            Manager Rachel = new Manager();
            Rachel.SetName("Rachel");
            Rachel.SetHeadPerson(Steve);
            Rachel.SetSubordinates(new List<IHandler> { Chuck, Denise });

            // assign all bosses/subs not already completed
            Steve.SetSubordinates(new List<IHandler> { Bob, Rachel });
            Jack.SetSubordinates(new List<IHandler> { John, Mary, Jane, Tom, Nick });
            Jack.SetHeadPerson(Bob);
            Jeff.SetSubordinates(new List<IHandler> { Rob, Ed, Rick, Michael });
            Jeff.SetHeadPerson(Bob);
            Chuck.SetSubordinates(new List<IHandler> { Joe, Sam, Frank, Greg });
            Chuck.SetHeadPerson(Rachel);
            Denise.SetSubordinates(new List<IHandler> { Amy, Wil, Nancy, Adam });
            Denise.SetHeadPerson(Rachel);

            // start the given scenario
            John.SeeDanger();

            Steve.OrderEvacuationOfSubordinates();

            // prevent immediate closure of the console 
            Console.ReadKey();
        }
    }
}