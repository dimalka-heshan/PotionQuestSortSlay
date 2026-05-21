using System.Collections.Generic;
using UnityEngine;

namespace BottleSystem
{
    public class BottleLogicTestRunner : MonoBehaviour
    {
        [ContextMenu("Run Pure Logic Tests")]
        public void RunTests()
        {
            Debug.Log("--- BOTTLE PURE LOGIC TESTS START ---");
            
            TestA();
            TestB();
            TestC();
            TestD();
            TestE();
            TestF();
            
            Debug.Log("--- BOTTLE PURE LOGIC TESTS END ---");
        }

        private void TestA()
        {
            // [Blue, Red, Blue, Red] into [] = [Blue, Red, Blue] and [Red], amount 1
            var source = CreateVirtualBottle(0, 4, new List<string> { "Blue", "Red", "Blue", "Red" });
            var target = CreateVirtualBottle(1, 4, new List<string> { });

            int amount = source.CalculatePourAmountTo(target);
            bool success = source.PourTo(target);

            bool pass = success && amount == 1 && source.DebugColors() == "[Blue, Red, Blue]" && target.DebugColors() == "[Red]";
            LogResult("Test A", pass, $"Final: {source.DebugColors()}, {target.DebugColors()}, Amount: {amount}");
            Cleanup(source, target);
        }

        private void TestB()
        {
            // [Blue, Red, Red, Red] into [] = [Blue] and [Red, Red, Red], amount 3
            var source = CreateVirtualBottle(0, 4, new List<string> { "Blue", "Red", "Red", "Red" });
            var target = CreateVirtualBottle(1, 4, new List<string> { });

            int amount = source.CalculatePourAmountTo(target);
            source.PourTo(target);

            bool pass = amount == 3 && source.DebugColors() == "[Blue]" && target.DebugColors() == "[Red, Red, Red]";
            LogResult("Test B", pass, $"Final: {source.DebugColors()}, {target.DebugColors()}, Amount: {amount}");
            Cleanup(source, target);
        }

        private void TestC()
        {
            // [Blue, Red, Red, Red] into [Red, Red, Red] = [Blue, Red, Red] and [Red, Red, Red, Red], amount 1
            var source = CreateVirtualBottle(0, 4, new List<string> { "Blue", "Red", "Red", "Red" });
            var target = CreateVirtualBottle(1, 4, new List<string> { "Red", "Red", "Red" });

            int amount = source.CalculatePourAmountTo(target);
            source.PourTo(target);

            bool pass = amount == 1 && source.DebugColors() == "[Blue, Red, Red]" && target.DebugColors() == "[Red, Red, Red, Red]";
            LogResult("Test C", pass, $"Final: {source.DebugColors()}, {target.DebugColors()}, Amount: {amount}");
            Cleanup(source, target);
        }

        private void TestD()
        {
            // [Blue, Red, Blue, Red] into [Blue] = Invalid
            var source = CreateVirtualBottle(0, 4, new List<string> { "Blue", "Red", "Blue", "Red" });
            var target = CreateVirtualBottle(1, 4, new List<string> { "Blue" });

            int amount = source.CalculatePourAmountTo(target);
            bool success = source.PourTo(target);

            bool pass = !success && amount == 0;
            LogResult("Test D", pass);
            Cleanup(source, target);
        }

        private void TestE()
        {
            // [Blue] into [Red] = Invalid
            var source = CreateVirtualBottle(0, 4, new List<string> { "Blue" });
            var target = CreateVirtualBottle(1, 4, new List<string> { "Red" });

            int amount = source.CalculatePourAmountTo(target);
            bool success = source.PourTo(target);

            bool pass = !success && amount == 0;
            LogResult("Test E", pass);
            Cleanup(source, target);
        }

        private void TestF()
        {
            // [] into [] = Invalid
            var source = CreateVirtualBottle(0, 4, new List<string> { });
            var target = CreateVirtualBottle(1, 4, new List<string> { });

            int amount = source.CalculatePourAmountTo(target);
            bool success = source.PourTo(target);

            bool pass = !success && amount == 0;
            LogResult("Test F", pass);
            Cleanup(source, target);
        }

        private BottleController CreateVirtualBottle(int index, int capacity, List<string> initial)
        {
            GameObject go = new GameObject("TestBottle_" + index);
            var bc = go.AddComponent<BottleController>();
            // We initialize without a view for pure logic testing
            bc.Initialize(index, capacity, initial);
            return bc;
        }

        private void Cleanup(params BottleController[] bottles)
        {
            foreach (var b in bottles) if (b != null) DestroyImmediate(b.gameObject);
        }

        private void LogResult(string name, bool success, string msg = "")
        {
            if (success) Debug.Log($"<color=green>PASS</color>: {name} {msg}");
            else Debug.LogError($"<color=red>FAIL</color>: {name} {msg}");
        }
    }
}
