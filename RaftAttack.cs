using Harmony;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace com.blargs.raft.raftattack
{
    [ModTitle("RaftAttack")]
    [ModDescription("Shows an alert when the shark is attacking the raft.\nIcon made by smalllikeart <https://www.flaticon.com/authors/smalllikeart> from https://www.flaticon.com/ is licensed by http://creativecommons.org/licenses/by/3.0/ \n\nSpecial thanks to TeigRolle for his work on SharkAlarm and his willingness to help with his knowledge of Unity and AssetBundles.")]
    [ModAuthor("Echo343")]
    [ModIconUrl("https://i.imgur.com/sCXgE7Q.png")]
    [ModWallpaperUrl("https://i.imgur.com/Hy0XmKb.png")]
    [ModVersion("1.0.0")]
    [RaftVersion("Update 9 (3602784)")]
    public class RaftAttack : Mod
    {

        private static GameObject canvas = null;
        private static Shark selectedShark = null;
        private static bool previousBittingState = false;

        private enum UIPos { TOPRIGHT, TOPRIGHT_OFFSET };
        private static UIPos iconPosition = UIPos.TOPRIGHT;
        private Vector3 offsetTranslation;
        private Vector3 reverseOffsetTranslation;

        private const string harmonyId = "com.blargs.raft.raftattack";
        private HarmonyInstance harmony = null;

        private AssetBundle bundle;
        private GameObject canvasPrefab;
        private const string ASSET_BUNDLE_PATH = "/mods/ModData/RaftAttack/raftattack.asset";
        private Image[] images;

        private Gradient colorG = new Gradient();
        private GradientColorKey black = new GradientColorKey(Color.black, 0f);
        private GradientColorKey red = new GradientColorKey(Color.red, 1f);

        private float num = 0f;


        private void Start()
        {
            harmony = HarmonyInstance.Create(harmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            RaftAttack.canvas = null;
            RaftAttack.selectedShark = null;
            RaftAttack.previousBittingState = false;
            RaftAttack.iconPosition = UIPos.TOPRIGHT;

            offsetTranslation = new Vector3(0, 115);
            reverseOffsetTranslation = new Vector3(0, -1);
            reverseOffsetTranslation.Scale(offsetTranslation);

            this.bundle = AssetBundle.LoadFromFile(Directory.GetCurrentDirectory() + RaftAttack.ASSET_BUNDLE_PATH);
            this.canvasPrefab = (GameObject)this.bundle.LoadAsset<GameObject>("UICanvas");
            colorG.colorKeys = new GradientColorKey[2]
            {
                black,
                red
            };
            RConsole.registerCommand(typeof(RaftAttack), "Cause the shark to attack the raft.", "attackRaftRaftAttack", new Action(this.AttackRaft));
            RConsole.registerCommand(typeof(RaftAttack), "Offsets the UI incase other mods are using the space.", "offsetUIRaftAttack", new Action(this.OffsetUI));
#if DEBUG
            RConsole.registerCommand(typeof(RaftAttack), "", "debugRaftRaftAttack", new Action(this.DebugStuff));
            RConsole.Log("RaftAttack loaded!");
#endif
        }

        public void OffsetUI()
        {
            if (canvas == null)
            {
                return;
            }

            switch (RaftAttack.iconPosition)
            {
                case UIPos.TOPRIGHT:
                    foreach (Image img in images)
                    {
                        img.gameObject.transform.Translate(reverseOffsetTranslation);
                    }
                    RaftAttack.iconPosition = UIPos.TOPRIGHT_OFFSET;
                    break;
                case UIPos.TOPRIGHT_OFFSET:
                    foreach (Image img in images)
                    {
                        img.gameObject.transform.Translate(offsetTranslation);
                    }
                    RaftAttack.iconPosition = UIPos.TOPRIGHT;
                    break;
            }
        }

        public void AttackRaft()
        {
            Shark shark = FindObjectOfType<Shark>();
            shark.ChangeState(SharkState.AttackRaft);
        }

        public void Update()
        {
            if (canvas == null)
            {
                RaftAttack.iconPosition = UIPos.TOPRIGHT;
                canvas = Instantiate<GameObject>(this.canvasPrefab);
                this.images = canvas.GetComponentsInChildren<Image>();
                canvas.SetActive(false);
#if DEBUG
                RConsole.Log("instantiating canvas");
#endif
            }
            else if (selectedShark != null)
            {
                if (selectedShark.bitingRaft)
                {
                    num = (selectedShark.targetBlock == null) ? 0f : selectedShark.targetBlock.NormalizedHealth;
                    for (int index = 0; index < images.Length; ++index)
                    {
                        if (images[index].name != "Image")
                        {
                            images[index].color = colorG.Evaluate(1f - num);
                        }
                    }
                }

                if (selectedShark.bitingRaft && !previousBittingState)
                {
#if DEBUG
                    RConsole.Log("Shark is biting block: " + selectedShark.targetBlock.name);
#endif
                    if (selectedShark.targetBlock.name.StartsWith("SharkBait"))
                    {
                        selectedShark = null;
                    }
                    else
                    {
                        canvas.SetActive(true);
                        previousBittingState = true;
                    }
                }
                else if (!selectedShark.bitingRaft && previousBittingState)
                {
                    canvas.SetActive(false);
                    selectedShark = null;
                }
            }
        }

        public void DebugStuff()
        {
            RConsole.Log("images: " + images.ToString());
            RConsole.Log("length: " + images.Length.ToString());
            for (int index = 0; index < images.Length; ++index)
            {
                RConsole.Log(images[index].name);
            }
        }

        public static void SetActive(Shark shark)
        {
            if (selectedShark == null)
            {
                selectedShark = shark;
                previousBittingState = false;
            }
        }

        public void OnModUnload()
        {
#if DEBUG
            RConsole.Log("RaftAttack has been unloaded!");
            RConsole.unregisterCommand("debugRaftRaftAttack");
#endif
            RConsole.unregisterCommand("attackRaftRaftAttack");
            RConsole.unregisterCommand("offsetUIRaftAttack");
            canvas = null;
            this.bundle.Unload(true);
            harmony.UnpatchAll(harmonyId);
            Destroy(gameObject);
        }

    }
}
