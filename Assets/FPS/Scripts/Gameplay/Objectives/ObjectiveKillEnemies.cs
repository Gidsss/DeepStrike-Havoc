using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveKillEnemies : Objective
    {
        [Tooltip("Chose whether you need to kill every enemies or only a minimum amount")]
        public bool MustKillAllEnemies = true;

        [Tooltip("If MustKillAllEnemies is false, this is the amount of enemy kills required")]
        public int KillsToCompleteObjective = 5;

        [Tooltip("Start sending notification about remaining enemies when this amount of enemies is left")]
        public int NotificationEnemiesRemainingThreshold = 3;

        [Tooltip("Time limit in seconds to complete the objective")]
        public float TimeLimitInSeconds = 600f; // 10 minutes

        private float currentTime;

        int m_KillTotal;

        protected override void Start()
        {
            base.Start();

            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);

            // Initialize the timer
            currentTime = TimeLimitInSeconds;

            // set a title and description specific for this type of objective, if it hasn't one
            if (string.IsNullOrEmpty(Title))
                Title = "Eliminate " + (MustKillAllEnemies ? "all the" : KillsToCompleteObjective.ToString()) +
                        " enemies";

            if (string.IsNullOrEmpty(Description))
                Description = GetUpdatedCounterAmount();
        }

        private void Update()
        {
            // Update the timer
            if (!IsCompleted)
            {
                currentTime -= Time.deltaTime;

                if (currentTime <= 0f)
                {
                    // Time limit exceeded, handle accordingly (e.g., fail the objective)
                    FailObjective("Time limit exceeded");
                }
            }
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (IsCompleted)
                return;

            m_KillTotal++;

            if (MustKillAllEnemies)
                KillsToCompleteObjective = evt.RemainingEnemyCount + m_KillTotal;

            int targetRemaining = MustKillAllEnemies ? evt.RemainingEnemyCount : KillsToCompleteObjective - m_KillTotal;

            // update the objective text according to how many enemies remain to kill
            if (targetRemaining == 0)
            {
                CompleteObjective(string.Empty, GetUpdatedCounterAmount(), "Objective complete : " + Title);
            }
            else if (targetRemaining == 1)
            {
                string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                    ? "One enemy left"
                    : string.Empty;
                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
            else
            {
                // create a notification text if needed, if it stays empty, the notification will not be created
                string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                    ? targetRemaining + " enemies to kill left"
                    : string.Empty;

                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
        }

        string GetUpdatedCounterAmount()
        {
            return m_KillTotal + " / " + KillsToCompleteObjective + "\nTime: " + FormatTime(currentTime);
        }

        string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
        }
        void FailObjective(string reason)
        {
            string detailedDescription = GetUpdatedCounterAmount();
            string failureReason = "Objective failed: " + Title;

            // Call the FailObjective method with the additional information
            FailObjective(reason, detailedDescription, failureReason);
        }
        void FailObjective(string reason, string detailedDescription, string failureReason)
        {
            Debug.LogError($"Objective failed: {failureReason}. Reason: {reason}. Details: {detailedDescription}");

        }

    }
}
