using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine.UI;

public class testTransaction : MonoBehaviour
{
    List<string> completeResult = new List<string>();

    const int kMaxLogSize = 16382;
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

    // Use this for initialization
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
        
        completeResult.Clear();
    }

    protected virtual void InitializeFirebase()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        // NOTE: You'll need to replace this url with your Firebase App's database
        // path in order for the database connection to work correctly in editor.
        app.SetEditorDatabaseUrl("https://sample-de6bc.firebaseio.com/");
        if (app.Options.DatabaseUrl != null)
            app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);

        Debug.Log("Ready");        
    }

    public void go()
    {
        StartCoroutine(transaction_IE());
    }

    IEnumerator transaction_IE()
    {
        Debug.Log("Start transaction");
        DatabaseReference counterRef = FirebaseDatabase.DefaultInstance.RootReference.Child("PLAYER_DATA").Child("TEST_PlayerCount");
        yield return new WaitForSeconds(2);
        counterRef.RunTransaction(AddTransaction).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("COMPLETE : " + task.Result);
                        completeResult.Add(task.Result.ToString());
                    }
                    else
                    {
                        Debug.Log("NOT COMPLETE : " + task.Exception.ToString());
                    }
                    Debug.Log("Transaction Done");
                });
    }

    public int targetInt = 0;

    TransactionResult AddTransaction(MutableData mutableData)
    {
        int theValue = -1;
        bool parseResult = int.TryParse(mutableData.Value.ToString(), out theValue);

        Debug.Log("------ Mutable Data : " + theValue);

        if (parseResult)
        {
            mutableData.Value = theValue + 1;
            Debug.Log("The Value : " + mutableData.Value);
            return TransactionResult.Success(mutableData);
        }
        else
        {
            Debug.Log("Failed : " + mutableData.Value.ToString());
            return TransactionResult.Abort();
        }
    }
}
