﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EyeXFramework;
using Tobii.EyeX.Framework;
using Tobii.EyeX.Client;
using System.Timers;

/*
 *  Class: FixationDetection
 *  Name: Richard Horne
 *  Date: 10/05/2015
 *  Description: This class is to encapsulate the required logic to detect an user eyes fixation on a point of the screen from the behaviors provided by the eyeX engine.
 *  Purpose: To enable code to be run when a user fixates on location on the screen. A method can be passed in and run once the fixation is confirmed.
 */

namespace GazeToolBar
{
    //State the Fixation detection can be in.
    public enum EFixationState { WaitingForInPutSelection, RunningFixationWithSelection };
    
    
    public class FixationDetection
    {
        //Timer to measure if a how long it has been since the fixation started. 
        private static Timer aTimer;
        //Reference to eyeXhost, will be instantiated on form or in another manager/worker class.
        public static EyeXHost eyeXHost;
        //Fixation data stream, used to attached to fixation events.
        public static FixationDataStream fixationPointDataStream;
        public ActionToRunAtFixation SelectedFixationAcion { get; set; }
        public int lengthOfTimeOfGazeBeforeRunningAction { get; set; }
        //State variable of FixationDetection class.
        EFixationState fixationState { get; set; }

        //Delegate to outline signature of the method that is run when a fixation is detected
        public delegate void ActionToRunAtFixation(int xpos, int ypos);

        //Field to record location of beginning fixation location.
        private int xPosFixation = 0;
        private int yPosFixation = 0;




        public FixationDetection(EyeXHost inputEyeXHost)
        {
            //Pass in eyeXhost from form\class to manage eye tracking system.
            eyeXHost = inputEyeXHost;
            fixationPointDataStream = eyeXHost.CreateFixationDataStream(FixationDataMode.Slow);
            EventHandler<FixationEventArgs> runSelectedActionAtFixationDelegate = new EventHandler<FixationEventArgs>(RunSelectedActionAtFixation);
            fixationPointDataStream.Next += runSelectedActionAtFixationDelegate;

            //Timer to run selected interaction with OS\aapplication user is trying to interact with, once gaze is longer than specified limit
            lengthOfTimeOfGazeBeforeRunningAction = 600;
            aTimer = new System.Timers.Timer(lengthOfTimeOfGazeBeforeRunningAction);
            aTimer.AutoReset = false;

            aTimer.Elapsed += runActionWhenTimerReachesLimit;
        }

        //This method of is run on gaze events, checks if it is the beginning or end of a fixation and runs appropriate code.
        private void RunSelectedActionAtFixation(object o, FixationEventArgs fixationDataBucket)
        {

            if(fixationState == EFixationState.RunningFixationWithSelection)
            {
                if(fixationDataBucket.EventType == FixationDataEventType.Begin)
                {
                    aTimer.Start();
                    xPosFixation = (int)Math.Floor(fixationDataBucket.X);
                    yPosFixation = (int)Math.Floor(fixationDataBucket.Y);
                    //Debug 
                    Console.WriteLine("Fixation Started X" + fixationDataBucket.X + " Y" + fixationDataBucket.Y);
                    
                } else if(fixationDataBucket.EventType == FixationDataEventType.End)
                {
                    aTimer.Stop();
                    //Debug
                    Console.WriteLine("Fixation Stopped");
                }
            }
        }

        //this action is run when timer reaches lengthOfTimeOfGazeBeforeRunningAction limit and the elapsed event is run. 
        //doing it this as we need a way of knowing when and where a fixation begins which the eyex provides, problem is it only provides when,
        //a fixation begins and where it ends, we have to build the logic to detect where it begins, check that its does not end in a specified time period,
        //and as long as it does not end, run the required action at that location from the beginning of the fixation.
        public void runActionWhenTimerReachesLimit(object o, ElapsedEventArgs e)
        {
            //Debug
            Console.WriteLine("Timer reached event, running required action");

            SelectedFixationAcion(xPosFixation, yPosFixation);
            fixationState = EFixationState.WaitingForInPutSelection;
        }


        public void SetupSelectedFixationAction(ActionToRunAtFixation inputActionToRun)
        {
            SelectedFixationAcion = inputActionToRun;
            fixationState = EFixationState.RunningFixationWithSelection;
        }
    }
}