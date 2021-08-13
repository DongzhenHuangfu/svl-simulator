/**
 * Copyright (c) 2019 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using UnityEngine;
using Simulator.Bridge;
using System.Collections;

namespace Simulator.Components
{
    public class BridgeClient : MonoBehaviour
    {
        public string Connection { get; private set; }

        public Status BridgeStatus => Bridge == null ? Status.Disconnected : Bridge.Status;

        public BridgeInstance Bridge { get; private set; }

        static readonly float reconnectInterval = 3.0f;

        Coroutine watchDog = null;
        public void Init(BridgePlugin plugin)
        {
            Bridge = new BridgeInstance(plugin);
        }

        public void Connect(string connection)
        {
            if (BridgeStatus != Status.Disconnected && BridgeStatus != Status.UnexpectedlyDisconnected)
            {
                Bridge.Disconnect();
            }

            Connection = connection;
        }

        private void OnEnable()
        {
            watchDog = StartCoroutine(ConnectionWatchDog());
        }

        private void OnDisable()
        {
            if (watchDog != null)
            {
                StopCoroutine(watchDog);
                watchDog = null;
            }
        }

        private void OnDestroy()
        {
            if (Bridge.Status != Status.Disconnected) Bridge.Disconnect();
        }

        private IEnumerator ConnectionWatchDog()
        {
            yield return new WaitUntil(() => Loader.Instance.Status != SimulatorStatus.Starting);
            Bridge.Connect(Connection);

            // in interactive mode we reconnect to the bridge if it gets disconnected.
            bool reconnect = Loader.Instance.CurrentSimulation.Interactive;

            while (Loader.Instance.Status == SimulatorStatus.Running)
            {
                if (BridgeStatus == Status.Connected)
                {
                    Bridge.Update();
                }
                else if (BridgeStatus == Status.UnexpectedlyDisconnected || BridgeStatus == Status.Disconnected)
                {
                    if (reconnect)
                    {
                        // do not report error, as doing so would cause WISE to terminate the simulation
                        Bridge.Disconnect();
                        yield return new WaitForSecondsRealtime(reconnectInterval);
                        Bridge.Connect(Connection);
                    }
                    else
                    {
                        Loader.Instance.reportStatus(SimulatorStatus.Error, "Bridge socket was unexpectedly disconnected");
                        break;
                    }
                }

                yield return null;
            }
            if (Bridge.Status != Status.Disconnected) Bridge.Disconnect();
        }
    }
}
