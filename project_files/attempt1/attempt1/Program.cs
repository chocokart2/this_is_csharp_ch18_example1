using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace attempt1
{
    [Serializable]
    class MemoListData
    {
        // 정의
        // 파일 입출력을 할때 사용되는 클래스입니다.
        // 모든 데이터가 여기에 저장될 것입니다.

        // 설명

        // 필드
//public:
        public List<MemoData> memoList;
        public string[] fileNameList;
//private:
        string directoryPath = "memo_files\\";

        // 프로퍼티

        // 함수
        public void UpdateFileList()
        {
            // 함수 설명
            // 특정한 파일 경로에 있는 .kart 바이너리 파일을 찾아서 파일 이름을 fileNameList에 집어넣습니다.
            // 에외
            // 특정한 폴더 경로가 존재하지 않으면 알잘딱깔센 경고를 남기고, 폴더를 만듭니다.
            // 인풋
            // 특정 파일 경로를 찾아 갑니다.
            // 아웃풋
            // fileNameList가 변경됩니다.

            // 파일에 접근하도록 시도
            // 해당 path에 폴더가 있는가
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            if (directoryInfo.Exists == false)
            {
                Console.WriteLine($"WARNING_MemoListData.UpdateFileList() : {directoryPath}에 해당하는 폴더가 존재하지 않습니다.");
                directoryInfo.Create();
            }
            
            
        }
        public void Ready()
        {

        }
    }

    [Serializable]
    class MemoData
    {

        int id;
        string Title;
        List<string> content;
    }



    internal class Program
    {


        static void Main(string[] args)
        {
            MemoListData self = new MemoListData();
            self.UpdateFileList();


            Console.ReadLine();
        }

        void ShowMain()
        {

        }
    }
}
