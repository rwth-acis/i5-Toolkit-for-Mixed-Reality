using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using i5.Toolkit.Core.ServiceCore;
using TMPro;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public abstract class ViveWandCore
    {
        public IViveWandShell shell;

        public IMixedRealityInputSource ownSource { set; get; }

        protected void ActivateDescriptionTexts()
        {
            string name = "ButtonDescriptions";
            shell.AddGameobjectToBuffer(name, name);
            shell.SetGameObjectActive(name, true);
            shell.RemoveGameobjectFromBuffer(name);
        }

        /// <summary>
        /// Disabel the description texts after descriptionShowTime seconds.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator DisableDescriptionsAfterShowTime()
        {
            yield return new WaitForSeconds(shell.GetToolSetup().descriptionShowTime);
            shell.SetGameObjectActive("ButtonDescriptions", false);
        }

        /// <summary>
        /// Coroutine to set the ownSourceVariable. Has to be done in a loop in a coroutine, because it can vary at which point the input source is registered in the InputSystem.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator SetOwnSource()
        {
            while (ownSource == null)
            {
                ownSource = GetOwnInputSource();
                yield return null;
            }
        }

        /// <summary>
        /// Sets the description texts for the TMP that is attached to an GameObject with name gameobjectName.
        /// </summary>
        /// <param name="gameobjectName"></param> The name of the object, the TMP is attached to
        /// <param name="text"></param> The text to be set
        /// <param name="defaulText"></param> The default trext, that is used, if text is "".
        protected void SetText(string gameobjectName, string text, string defaulText)
        {
            shell.AddGameobjectToBuffer("ButtonDescriptions/" + gameobjectName, gameobjectName);
            shell.SetGameObjectActive(gameobjectName, true);
            if (text != "")
            {
                shell.SetTMPText(gameobjectName,text);
            }
            else if (defaulText != "")
            {
                shell.SetTMPText(gameobjectName, defaulText);
            }
            else
            {
                shell.SetGameObjectActive(gameobjectName, false);
            }
            shell.RemoveGameobjectFromBuffer(gameobjectName);
        }

        /// <summary>
        /// Is the provided input source the same as the object this belongs to?
        /// </summary>
        /// <param name="inputSource"></param>
        /// <returns></returns>
        protected bool IsInputSourceThis(IMixedRealityInputSource inputSource)
        {
            //return this == inputSource.Pointers[0]?.Controller?.Visualizer?.GameObjectProxy?.GetComponentInChildren<ViveWand>();
            return inputSource == ownSource;
        }

        /// <summary>
        /// Get the input source, this object belongs to. Can return null, when the input source isn't registerd yet.
        /// </summary>
        /// <returns></returns>
        protected IMixedRealityInputSource GetOwnInputSource()
        {
            foreach (var source in shell.GetInputSources())
            {
                foreach (var pointer in source.Pointers)
                {
                    if (shell.GameObjectProxyEqualsOwnObject(pointer.Controller?.Visualizer))
                    {
                        return source;
                    }
                }
            }
            return null;
        }
    }
}
