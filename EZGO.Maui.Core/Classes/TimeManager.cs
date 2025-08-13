using System;
using System.Collections.Generic;
using System.Diagnostics;
using EZGO.Maui.Core.Models.Instructions;

namespace EZGO.Maui.Core.Classes
{
    public class TimeManager
    {
        private readonly Stopwatch st;
        private readonly Dictionary<int, InstructionViewedTimeModel> instructionsViewed;

        public InstructionItem CurrentInstruction { get; set; }

        public TimeManager()
        {
            st = new Stopwatch();
            instructionsViewed = new Dictionary<int, InstructionViewedTimeModel>();
        }

        public void StartTimer()
        {
            st.Start();
        }

        public void Stop()
        {
            st.Stop();
            if (CurrentInstruction != null)
                HandleMesuredTime();
        }

        public void Restart()
        {
            st.Restart();
        }

        private void HandleMesuredTime()
        {
            if (instructionsViewed.TryGetValue(CurrentInstruction.Id, out var instruction))
            {
                instruction.TimeSpend += st.ElapsedMilliseconds;
                Debug.WriteLine($"Time spended on instruction {instruction.InstructionId}: {instruction.TimeSpend} ms");
            }
            else
            {
                instructionsViewed.Add(CurrentInstruction.Id, new InstructionViewedTimeModel(CurrentInstruction.Id, st.ElapsedMilliseconds));
            }
            // Send measured time to api of firebase
            Debug.WriteLine($"Time spended on instruction: {st.ElapsedMilliseconds} ms");
        }
    }
}
