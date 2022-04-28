using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class CSVWriter : MonoBehaviour
{
    CsvExporter csvExporter;
    // Start is called before the first frame update
    void Start()
    {
        //csvExporter = new CsvExporter("test");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class CsvExporter
{
    List<string[]> csvData;
    string[] header;
    string path;
    public CsvExporter(string filename)
    {
        csvData= new List<string[]>(); // CSVの中身を入れるリスト
        header =new string[]{"time", "mode", "modeDetail", "obj", "floor", "tapCount", "tapPhase", "loc_x", "loc_y"};
        path = UnityEngine.Application.persistentDataPath + "/" + filename + ".csv";

        //string[] data = { "1", "2", "3", "4", "5", "6", "7", "8" };
        //string[] data2 = { "2", "apple", "banana", "mikan" };

        CheckExistCsv();
        WriteCsv(header);
        CheckExistCsv();

        //AppendCSV(data);
    }

    public bool CheckExistCsv()
    {
        if (System.IO.File.Exists(path))
        {
            Debug.Log("CSVファイルが存在するので追記します");
            return true;
        }
        else
        {
            Debug.Log("CSVファイルが存在しないので作成します");
            return false;
        }
    }

    public void WriteCsv(string[] header)
    {
        StreamWriter sw = new StreamWriter(path, false, Encoding.GetEncoding("UTF-8"));

        string h = string.Join(",", header);
        sw.WriteLine(h);
        sw.Close();

        Debug.Log("CSV is written: "+ path);
    }

    public void AppendCSV(string[] data)
    {
        StreamWriter sw = new StreamWriter(path, true, Encoding.GetEncoding("UTF-8"));

        string d = string.Join(",", data);

        sw.WriteLine(d);

        sw.Close();

        Debug.Log("CSV is Appended: " + data);
    }

    //csvファイルの読み出し
    public List<string[]> ReadCSV(string path)
    {
        // ファイル読み込み
        // 引数説明：第1引数→ファイル読込先, 第2引数→エンコード
        //Unityのプロジェクトフォルダー内にある場合
        StreamReader sr = new StreamReader(path, Encoding.GetEncoding("UTF-8"));
        string line;

        // 行がnullじゃない間(つまり次の行がある場合は)、処理をする。→最後の行まで読みだす。
        while ((line = sr.ReadLine()) != null)
        {
            // コンソールに出力
            Debug.Log(line);
            csvData.Add(line.Split(','));
        }

        // StreamReaderを閉じる
        sr.Close();

        Debug.Log(path);

        return csvData;
    }
}
