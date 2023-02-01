using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class MotionLogger : MonoBehaviour
{
    [SerializeField] GameObject leftController;
    [SerializeField] GameObject rightController;

    StreamWriter sw;

    static string mResourcesPass = "Assets/Resources/MotionData/";

    void Awake()
    {
        //experimentController = GetComponent<ExperimentController>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
 
    }

    public interface ICSVParse
    {
        void CSVParse(string[] data);
        string CSVSave();
    }

    public static void CSVSave<T>(IEnumerable<T> inSaveDatas, string inSaveDataPass = "SaveData.csv") where T : ICSVParse
    {
        try
        {
            using (var sw = new System.IO.StreamWriter(mResourcesPass + inSaveDataPass, false))
            {
                foreach (var data in inSaveDatas)
                {
                    sw.WriteLine(data.CSVSave());
                }
            }
        }
        catch (System.Exception e) { }
    }
}
