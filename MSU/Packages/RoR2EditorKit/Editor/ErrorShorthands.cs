using System;

namespace RoR2EditorKit
{
    /// <summary>
    /// Shorthands for throwing Errors.
    /// </summary>
    public static class ErrorShorthands
    {
        /// <summary>
        /// Returns a null reference exception with the following message:
        /// <para>"Field <paramref name="fieldName"/> cannot be Empty or Null"</para>
        /// </summary>
        /// <param name="fieldName">The name of the field that's empty or null</param>
        /// <returns>The null reference exception</returns>
        public static NullReferenceException NullString(string fieldName)
        {
            return new NullReferenceException($"Field {fieldName} cannot be Empty or Null");
        }

        /// <summary>
        /// Returns a null reference exception with the following message:
        /// <para>"Your TokenPrefix in the RoR2EditorKit settings is Empty or Null"</para>
        /// </summary>
        /// <returns>The null reference exception</returns>
        public static NullReferenceException NullTokenPrefix()
        {
            return new NullReferenceException($"Your TokenPrefix in the RoR2EditorKit settings is Empty or Null");
        }

        /// <summary>
        /// Returns a null reference exception with the following message:
        /// <para>Your Main Manifest in the RoR2EditorKit Settings is Empty</para>
        /// </summary>
        /// <returns>The null reference exception</returns>
        public static NullReferenceException NullMainManifest()
        {
            return new NullReferenceException($"Your Main Manifest in the RoR2EditorKit Settings is Empty");
        }
    }
}