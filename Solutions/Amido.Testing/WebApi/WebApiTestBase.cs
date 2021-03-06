﻿using System;
using System.Collections.Generic;
using Amido.Testing.WebApi.Request;
using Microsoft.VisualStudio.TestTools.WebTesting;
using Amido.Testing.WebApi.ValidationRules;
using System.Threading;

namespace Amido.Testing.WebApi
{
    /// <summary>
    /// Base class required for using the Amido.Testing.WebApi helpers.
    /// </summary>
    public abstract class WebApiTestBase : WebTest
    {
        #region Declarations

        private TestRequests testRequests;
        private WebTestRequest currentRequest;
        private TestRequest currentTestRequest;
        private Outcome currentWebTestOutcome;
        private Func<string> commentDelegate;

        #endregion

        #region Properties

        /// <summary>
        /// Returns a new <see cref="TestTasks"/> collection for storing and running a multithreaded list of tasks.
        /// </summary>
        public TestTasks TestTasks
        {
            get
            {
                return new TestTasks();
            }
        }

        /// <summary>
        /// Returns a new <see cref="TestRequests"/> collection for storing web api request actions.
        /// </summary>
        public TestRequests Requests
        {
            get
            {
                if (testRequests == null)
                {
                    testRequests = new TestRequests();
                }

                return testRequests;
            }
        } 

        #endregion

        #region Constructor
        
        protected WebApiTestBase()
        {
            PreWebTest += OnPreWebTest;
            PostWebTest += OnPostWebTest;
        } 

        #endregion

        #region Start and clean up

        /// <summary>
        /// Start up tasks, tasks are run in the pre web test event.
        /// </summary>
        public virtual void StartUp()
        {
        }

        /// <summary>
        /// Clean up tasks, tasks are run in the post web test event.
        /// </summary>
        public virtual void CleanUp()
        {
        } 

        #endregion

        #region Web Test Requests

        /// <summary>
        /// Web Test Requests.
        /// </summary>
        public abstract void WebTestRequests();

        /// <summary>
        /// Main Enumerator for the <see cref="WebTest"/>.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<WebTestRequest> GetRequestEnumerator()
        {
            WebTestRequests();

            foreach (var testRequest in testRequests.Requests)
            {
                currentTestRequest = testRequest;
                currentRequest = testRequest.Request();
                currentWebTestOutcome = Outcome;
                currentRequest.ValidationRuleReferences.Clear();

                foreach (var assert in testRequest.Asserts)
                {
                    currentRequest.ValidateResponse += assert().Validate;
                }

                this.AddCommentToResult("Blah");

                if (testRequest.MaxRetries == 0)
                {
                    if (testRequest.WaitPeriod > 0)
                    {
                        Thread.Sleep(testRequest.WaitPeriod);
                    }
                    yield return currentRequest;
                }
                else
                {
                    var retryValue = testRequest.RetryValue;
                    if (testRequest.RetryValueDelegate != null)
                    {
                        retryValue = testRequest.RetryValueDelegate.Invoke();
                    }

                    currentRequest.PostRequest += CurrentRequestOnPostRequest;

                    for (var i = 0; i <= testRequest.MaxRetries; i++)
                    {
                        //TestValidateRetry(testRequest, i);

                        currentRequest.ValidateResponse += new AssertRetryValidationRule(testRequest.RetryTestType, retryValue).Validate;

                        yield return currentRequest;


                        if (currentTestRequest.RetryTestType == RetryTestType.StatusCodeEquals && (int)LastResponse.StatusCode == int.Parse(retryValue))
                        {
                            break;
                        }

                        if (currentTestRequest.RetryTestType == RetryTestType.BodyEquals && LastResponse.BodyString == retryValue)
                        {
                            break;
                        }

                        if (currentTestRequest.RetryTestType == RetryTestType.BodyIncludes && LastResponse.BodyString.IndexOf(retryValue, StringComparison.Ordinal) > -1)
                        {
                            break;
                        }

                        if (currentTestRequest.RetryTestType == RetryTestType.BodyDoesNotInclude && LastResponse.BodyString.IndexOf(retryValue, StringComparison.Ordinal) == -1)
                        {
                            break;
                        }

                        Thread.Sleep(testRequest.Interval);
                    }
                }
            }
            
            AddFinalOutput();
        }

        private void AddFinalOutput()
        {
            if (commentDelegate != null)
            {
                AddCommentToResult(commentDelegate.Invoke());
            }
        }

        #endregion

        public void FinalOutput(Func<string> commentDelegate)
        {
            this.commentDelegate = commentDelegate;
        }

        #region Test Helpers

        private void OnPreWebTest(object sender, PreWebTestEventArgs preWebTestEventArgs)
        {
            try
            {
                StartUp();
            }
            catch (Exception ex)
            {
                preWebTestEventArgs.WebTest.Outcome = Outcome.Fail;
                preWebTestEventArgs.WebTest.AddCommentToResult("Startup failed: " + ex.ToString());
            }

        }

        private void OnPostWebTest(object sender, PostWebTestEventArgs postWebTestEventArgs)
        {
            try
            {
                CleanUp();
                testRequests = null;
            }
            catch (Exception ex)
            {
                postWebTestEventArgs.WebTest.Outcome = Outcome.Fail;
                postWebTestEventArgs.WebTest.AddCommentToResult("Cleanup failed: " + ex.ToString());
            }
        }

        private void CurrentRequestOnPostRequest(object sender, PostRequestEventArgs postRequestEventArgs)
        {
            if (currentWebTestOutcome == Outcome.Pass)
            {
                postRequestEventArgs.WebTest.Outcome = Outcome.Pass;
            }
        }

        private void TestValidateRetry(TestRequest testRequest, int i)
        {
            if (i == 0)
            {
                currentRequest.Url = currentRequest.Url + "/askj";
            }

            if (i == testRequest.MaxRetries - 1)
            {
                currentRequest.Url = currentRequest.Url.Replace("/askj", "");
            }
        }

        #endregion
    }
}
