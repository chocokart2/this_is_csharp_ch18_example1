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
        // .kart 바이너리 파일에서 읽어들인 데이터들을 이 객체에 저장하는 역할을 맡습니다.

        // 필드
//public:
        public List<MemoData> memoList;
        public string[] fileNameList;
//private:
        int pageNum = 0;
        string directoryPath = "memo_files\\";
        readonly string commandSigniture = "!";
        ViewMode mode = ViewMode.Main;


        // 스트링 값을 받아서 함수를 돌려주는 녀석이 없을까?
        // 함수의 이름을 스트링 값으로 받고, 매개변수는 호출자가 알아서 하세요
        Dictionary<string, Command> keyValuePairs = new Dictionary<string, Command>();


        // 프로퍼티

        // 함수
//public:
        public void UpdateFileList()
        {
            // 함수 설명
            // 특정한 파일 경로에 있는 .kart 바이너리 파일을 찾아서 파일 이름을 fileNameList에 집어넣습니다.
            // 에외
            // (해결) 특정한 폴더 경로가 존재하지 않으면 알잘딱깔센 경고를 남기고, 폴더를 만듭니다.
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
            // 해당 폴더의 .chokart 이진 파일들 죄다 찾아 저장함.
            // 그 이후 파일명 죄다 저장한다.

            //directoryInfo.GetFiles()

            
        }
        public void Ready()
        {

        }
//private:
        void Init()
        {
            keyValuePairs.Add("다음", new Command(ShowNextPage));

        }
        void ShowNextPage(params string[] parameter)
        {
            pageNum = int.Parse(parameter[0]);
        }

        public Command ReceveCommand(string commandLine)
        {
            // 함수 설명
            // 명령어 스트링으로 추정되는 명령어 라인을 매개변수로 받는다
            // 명령어인지 체크한다
            // 명령어 신호를 덜어내고, 유효한 명령어 + 빈칸 ' '을 기준으로 명령어와 매개변수를 구분해낸다.
            // 맞는 함수를 리턴한다.

            // 시그너쳐를 통해 명령어인지 판정한다
            if(commandLine.StartsWith(commandSigniture) == false) // 명령어가 아닌 경우
            {
                Console.WriteLine("올바른 명령어가 아닙니다. !help 혹은 !도움 을 입력하세요");
                return null;
            }

            // 명령어 부분을 제거한다
            commandLine.Remove(0, commandSigniture.Length);

            switch (mode)
            {
                case ViewMode.Main:
                    switch (commandLine)
                    {
                        case "어":
                            break;
                        default: return null;
                    }
                    break;

                case ViewMode.Edit:
                    break;
                default:
                    return null;
                    break;
            }




        }

        // 열거형
        enum ViewMode // 현재 플레이어가 보고 있는 씬의 이름을 나타냅니다.
        {
            Main    = 0,
            Edit    = 1
        }

        // 대리자
        delegate void Command(params string[] parameter);
    }

    [Serializable]
    class MemoData
    {
        int id;
        string Title;
        List<string> content;
    }



    interface ICommand
    {
        
    }

    internal class Program // 메인 함수가 있는 클래스입니다.
    {
        // 메인 함수
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
