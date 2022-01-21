using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core;
using RoR2EditorKit.Core.Windows;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class CreateSha256HashAssetWindow : CreateRoR2ScriptableObjectWindow<Sha256HashAsset>
    {
        public Sha256HashAsset hashAsset;

        private SHA256 hasher;

        private Vector3Int topRow;
        private Vector3Int middleRow;
        private Vector3Int bottomRow;

        [MenuItem(Constants.RoR2EditorKitContextRoot + "ScriptableObjects/Sha256HashAsset", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateSha256HashAssetWindow>(null, "Create Sha256HashAsset");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
            hashAsset = (Sha256HashAsset)ScriptableObject;
            hasher = SHA256.Create();
            topRow = Vector3Int.zero;
            middleRow = Vector3Int.zero;
            bottomRow = Vector3Int.zero;
            mainSerializedObject = new SerializedObject(hashAsset);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            nameField = EditorGUILayout.TextField("Sha256HashAsset Name", nameField);

            topRow = EditorGUILayout.Vector3IntField("Top Row", topRow);
            middleRow = EditorGUILayout.Vector3IntField("Middle Row", middleRow);
            bottomRow = EditorGUILayout.Vector3IntField("Bottom Row", bottomRow);

            if (GUILayout.Button("Create Sha256HashAsset"))
            {
                var result = CreateHashAsset();
                if (result)
                {
                    Debug.Log($"Succesfully Created HashAsset {nameField}");
                    TryToClose();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private bool CreateHashAsset()
        {
            actualName = GetCorrectAssetName(nameField);
            try
            {
                if (string.IsNullOrEmpty(actualName))
                    throw ErrorShorthands.ThrowNullAssetName(nameof(nameField));

                hashAsset.name = actualName;

                List<int> sequence = new List<int>();
                sequence.Add(topRow.z);
                sequence.Add(middleRow.z);
                sequence.Add(bottomRow.z);
                sequence.Add(topRow.y);
                sequence.Add(middleRow.y);
                sequence.Add(bottomRow.y);
                sequence.Add(topRow.x);
                sequence.Add(middleRow.x);
                sequence.Add(bottomRow.x);

                byte[] array = new byte[sequence.ToArray().Length];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = (byte)sequence[i];
                }

                hashAsset.value = Sha256Hash.FromBytes(hasher.ComputeHash(array));

                Util.CreateAssetAtSelectionPath(hashAsset);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while creating artifact: {e}");
                return false;
            }
        }
    }
}
