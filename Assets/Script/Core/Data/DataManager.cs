using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Artoncode.Core.Utility;

namespace Artoncode.Core.Data {
	public class DataManager {
		private static DataManager manager;
		private List<string> saveFolders;
		private string defaultFilename;
		private string aesPassword;
		private string aesSalt;
		private string aesIV;
		private Hashtable data;
		private bool useAESEncryption;
		private bool useHMAC;
		
		private DataManager () {
			#if UNITY_IOS
			Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
			#endif

			reset ();

			defaultFilename = "userData.dat";
			aesPassword = "6vIVdQnQ4YGXA1cwe7avwmnTV441eylY66UdcJcWdkg6ZQfnxC9YdfqVlETKxPU";
			aesSalt = "AwMHFnYEb5Ezm0H6kJnXRsLDpAMkO2AyUr47QwOen5VQnxF242ahPi390RxiSdx";
			useAESEncryption = false;
			useHMAC = false;
		}
		
		public static DataManager defaultManager {
			get {
				if (manager == null) {
					manager = new DataManager ();
					manager.load ();
				}
				return manager;
			}
		}

		public static bool hasInstance () {
			return manager != null;
		}

		public static DataManager create () {
			return new DataManager ();	
		}

		public void setSaveFolders (params string[] folders) {
			saveFolders = new List<string> ();
            saveFolders.AddRange(folders);
		}

		public void setDefaultFilename (string filename) {
			defaultFilename = filename;
		}

		public void setEncryptionSetting (string password, string salt) {
			aesPassword = password;
			aesSalt = salt;
		}

		public void reset () {
			data = new Hashtable ();
		}

		#region Accessor 
		
		public void setInt (string key, ObscuredInt value) {
			data [key] = value;
		}
		
		public ObscuredInt getInt (string key) {
			if (!data.ContainsKey (key)) {
				return null;
			}
			object value = data [key];
			if (value is ObscuredInt)
				return (ObscuredInt)value;
			return null;
		}

		public void setFloat (string key, ObscuredFloat value) {
			data [key] = value;
		}
		
		public ObscuredFloat getFloat (string key) {
			if (!data.ContainsKey (key)) {
				return null;
			}
			object value = data [key];
			if (value is ObscuredFloat)
				return (ObscuredFloat)value;
			return null;
		}
		
		public void setDouble (string key, ObscuredDouble value) {
			data [key] = value;
		}
		
		public ObscuredDouble getDouble (string key) {
			if (!data.ContainsKey (key)) {
				return null;
			}
			object value = data [key];
			if (value is ObscuredDouble)
				return (ObscuredDouble)value;
			return null;
		}
		
		public void setBool (string key, ObscuredBool value) {
			data [key] = value;
		}
		
		public ObscuredBool getBool (string key) {
			if (!data.ContainsKey (key)) {
				return null;
			}
			object value = data [key];
			if (value is ObscuredBool)
				return (ObscuredBool)value;
			return null;
		}
		
		public void setString (string key, string value) {
			data [key] = value;
		}
		
		public string getString (string key) {
			if (!data.ContainsKey (key)) {
				return null;
			}
			return data [key].ToString ();
		}

		public void setObject (string key, object value) {
			data [key] = value;	
		}

		public object getObject (string key) {
			if (!data.ContainsKey (key)) {
				return null;
			}
			return data [key];
		}

		public Type getObjectType (string key) {
			if (!data.ContainsKey (key)) {
				return null;
			}
			return data [key].GetType ();
		}

		public void deleteKey (string key) {
			data.Remove (key);
		}

		#endregion

		public void save (string filename=null) {
			if (saveFolders == null) {
				saveFolders = new List<string> ();
				return;
			}

			// Serializing data
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, data);

			// Encrypt serialized data
			byte[] cipherData = null;
			cipherData = useAESEncryption ? AESEncryption.encrypt (ms.ToArray (), aesPassword, aesSalt) : ms.ToArray ();

			if (filename == null) {
				filename = defaultFilename;			
			}
			
			// Compute hmac to lock data on specific device
			string hmac = "";
			if (useHMAC) {
				hmac = Digest.computeHMACSHA1 (cipherData, SystemInfo.deviceUniqueIdentifier);
			}

			foreach (string folder in saveFolders) {
				// Write encrypted data
				FileStream fs = File.Create (folder + filename);
				fs.Write (cipherData, 0, cipherData.Length);
				fs.Close ();

				if (useHMAC) {
					fs = File.Create (folder + filename + ".hash");
					fs.Close ();

					StreamWriter sw = new StreamWriter(folder + filename + ".hash", false);
					sw.WriteLine(hmac);
					sw.Close ();
				}
			}
		}
		
		public bool load (string filename=null) {
			if (saveFolders == null) {
				reset ();
				Debug.Log ("No save folder defined");
				return false;
			}

			if (filename == null) {
				filename = defaultFilename;
			}

			bool successLoad = false;
			foreach (string folder in saveFolders) {
				// Read encrypted data
				string path = folder + filename;
				if (!Directory.Exists (folder)) {
					Directory.CreateDirectory (folder);
				}
				FileStream fs = File.Open (path, FileMode.OpenOrCreate);
				byte[] cipherData = new byte[fs.Length];
				fs.Read (cipherData, 0, (int)fs.Length);
				fs.Close ();
				
				if (cipherData.Length == 0) {
					reset ();
					Debug.Log ("No Such a file at:" + path);
				}
				else {
					// Verify that loaded data is from valid device
					string hmac = Digest.computeHMACSHA1 (cipherData, SystemInfo.deviceUniqueIdentifier);
					string hmacPath = folder + filename + ".hash";
					string storedHmac = "";
					if (!useHMAC || File.Exists (hmacPath)) {
						if (useHMAC) {
							StreamReader sr = File.OpenText (hmacPath);
							storedHmac = sr.ReadLine();
						}

						if (!useHMAC || hmac == storedHmac) {
							// Decrypt encrypted data
							byte[] plainData = null;
							plainData = useAESEncryption ? AESEncryption.decrypt (cipherData, aesPassword, aesSalt) : cipherData;
							
							// Deserialized decrypted data
							try {
								BinaryFormatter bf = new BinaryFormatter ();
								MemoryStream ms = new MemoryStream (plainData);
								data = (Hashtable)bf.Deserialize (ms);
								successLoad = true;
								break;
							} catch (SerializationException) {
								reset ();
								Debug.Log ("Can't deserialize data at:" + path);
							}
						}
						else {
							// Invalid device, erase the data
							reset ();
							Debug.Log ("File has been corrupted:" + path);
						}
					}
					else {
						reset ();
						Debug.Log ("Hmac for this data has been lost;" + path);
					}
				}
			}
			return successLoad;
		}
		
		public void debug () {
			foreach (DictionaryEntry kv in data) {
				Debug.Log (kv.Key + ": " + kv.Value.ToString ());
			}
		}
	}

}