﻿using System;
using System.Collections;
using System.Collections.Generic;

public class DiginoteTradingSystem : MarshalByRefObject, IDiginoteTradingSystem
{
    private double quotation; // current quotation of diginotes
    
    private List<User> usersList;
    private List<User> loggedUsers;

    private ArrayList sellOrders; // list of sell orders
    private ArrayList buyOrders; // list of buy orders

    private Logger logger; // log system

    public event ChangeDelegate ChangeEvent; // event to warn clients 
                                                    // when quotation changes

    private DiginoteDatabase diginoteDB; // diginote db

    public DiginoteTradingSystem()
    {
        // TODO check for log and load state

        // set initial quotation
        quotation = 1.0;

        // create order lists
        buyOrders = new ArrayList();
        sellOrders = new ArrayList();
        usersList = new List<User>();
        loggedUsers = new List<User>();

        // create logger
        logger = new Logger(ChangeEvent);
        Console.WriteLine("[DiginoteSystem] sup");
        diginoteDB = new DiginoteDatabase();
    }

    public override object InitializeLifetimeService()
    {
        Console.WriteLine("[Server]: Initialized Lifetime Service");
        return null;
    }

    public double GetQuotation()
    {
        return quotation;
    }

    private void SetNewQuotation(double value)
    {
        if(value > quotation) {
            Console.WriteLine("[Server]: Quotation value went up to: " + value);
            ChangeEvent(new ChangeArgs(ChangeType.QuotationUp, value));
        }
        else {
            Console.WriteLine("[Server]: Quotation value went down to: " + value);
            ChangeEvent(new ChangeArgs(ChangeType.QuotationDown, value));
        }

        // update quotation
        quotation = value;
        
    }

    public void AddBuyOrder(Order newOrder)
    {
        Console.WriteLine("[Server]: Added buy order from user " + newOrder.User);
        buyOrders.Add(newOrder);
    }

    public void AddSellOrder(Order newOrder)
    {
        Console.WriteLine("[Server]: Added sell order from user " + newOrder.User);
        sellOrders.Add(newOrder);
    }


    public bool userExists(User newUser){
        return usersList.Exists(user => user.Username == newUser.Username);
    }

    public bool RegisterUser(User newUser)
    {
        if (userExists(newUser))
        {
            Console.WriteLine("[DiginoteSystem] register failed: {0}", newUser.Username);
            return false;
        }

        usersList.Add(newUser);
        Console.WriteLine("[DiginoteSystem] user registered: {0}", newUser.Username);

       //SAVE STATE

        return true;
    }

    public bool checkLogin(User loguser)
    {
        return usersList.Exists(user => user.Username == loguser.Username);
    }

    public bool Login(User user) //DIFFERENT RETURNS NEEDED?
    {
        if (!checkLogin(user))
        {
            Console.WriteLine("[DiginoteSystem] login failed in: {0}", user.Username);
            return false;
        }
            

        loggedUsers.Add(new User(user.Name, user.Username, user.Password));
        Console.WriteLine("[DiginoteSystem] user logged in: {0}", user.Username);

        return true;
    }

    public List<User> getLoggedUsers()
    {
        return loggedUsers;
    }

    public void Logout(User loguser)
    {
        loggedUsers.Remove(loggedUsers.Find(user => user.Username == loguser.Username));
        Console.WriteLine("[DiginoteSystem] user logged out: {0}", loguser.Username);
    }

    // this function must be called to when something occurs
    //  and we need to call event, the thread is needed to
    //  prevent dead locks on server and client!
    private void safeInvoke(ChangeArgs args)
    {
        if (ChangeEvent != null)
        {
            Delegate[] invocationList = ChangeEvent.GetInvocationList();

            foreach (ChangeDelegate changeDelegate in invocationList)
            {
                try
                {
                    changeDelegate.BeginInvoke(args, null, null);
                }
                catch (Exception)
                {
                    ChangeEvent -= changeDelegate;
                }
            }
        }
    }
}