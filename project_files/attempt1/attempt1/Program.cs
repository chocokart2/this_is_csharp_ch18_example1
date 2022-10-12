using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace attempt1
{


    // 대리자
    delegate void Command(string parameter);

    // 클래스
    [Serializable]
    class MemoListData
    {
        // 정의
        // 파일 입출력을 할때 사용되는 클래스입니다.
        // 모든 데이터가 여기에 저장될 것입니다.

        // 설명
        // .kart 바이너리 파일에서 읽어들인 데이터들을 이 객체에 저장하는 역할을 맡습니다.

        // 생성자
        public MemoListData()
        {
            // 필드 초기화
            pageNum = 0;
            maxPageNum = 0;
            memoList = new List<MemoData>();
            NextCommand = new Queue<CommandCaller>();
            mainSceneCommands = new Dictionary<string, Command>();
            mainSceneCommands.Add("다음", new Command(ShowNextList));
            mainSceneCommands.Add("응답", new Command(RespondCommand));
            mainSceneCommands.Add("생성", new Command(MakeMemo));
            noteSceneCommands = new Dictionary<string, Command>();
            


            // 마무리 코드
            UpdateFileList();
        }
        // 필드
//public:
        public List<MemoData> memoList;
        public string[] fileNameList;
//private:
        int pageNum; // 0부터 시작하는 페이지 번호입니다.
        int maxPageNum;
        int lastId // 새로운 메모에 아이디를 부여할 때 사용합니다.
        {
            get
            {
                int returnValue = 0;
                for(int index = 0; index < memoList.Count; index++)
                {
                    if (memoList[index].id > returnValue) returnValue = memoList[index].id;
                }
                return returnValue;
            }
        }
        bool workLoop = true;
        string directoryPath = "memo_files\\";
        string fileType = ".chokart";
        readonly string commandSigniture = "!";
        ViewMode mode = ViewMode.Main;
        Queue<CommandCaller> NextCommand;
        //List<CommandCaller> NextCommand; // 함수 괄호를 빠져나오고 실행할 함수입니다. 불필요한 함수 호출 스택 채우기를 하지 않도록 하기 위합입니다.
        // 스트링 값을 받아서 함수를 돌려주는 녀석이 없을까?
        // 함수의 이름을 스트링 값으로 받고, 매개변수는 호출자가 알아서 하세요
        Dictionary<string, Command> mainSceneCommands; // 메인 화면에서 사용할 명령어입니다.
        Dictionary<string, Command> noteSceneCommands; // 노트 수정 화면에서 사용할 화면입니다.


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
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            

            //directoryInfo.GetFiles()

            
        }
        public void Work()
        {
            // 함수 설명
            // 명령어를 듣고 명령에 해당하는 함수를 호출합니다.
            string recevedString;
            // 호출할 함수가 없음 -> 명령어를 듣는 상태로 변경

            // 초기 파트


            for(; ; )
            {
                if (workLoop == false) break;

                recevedString = Console.ReadLine();
                if(recevedString != "나가기")
                {
                    ReceveCommand(recevedString);
                }
                else
                {
                    break;
                }
            }



            // 명령어를 듣기 -> (함수를 호출하기 -> 다음 함수를 호출 스텍에 넣기)
        }
        public void Kill()
        {
            // 명령 / 응답 루프를 종료합니다.
            

            workLoop = false;
        }
        //private:

        // "!응답"
        void RespondCommand(string parameter)
        {
            Console.WriteLine("A!");
        }

#warning 테스트 하지 않은 함수입니다
        // "!생성 [제목]"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <exception cref="CannotMakeAlternativeNameException"></exception>
        void MakeMemo(string parameter)
        {
            // 메모를 만들고, 그 메모를 수정하도록 합니다.
            // 입력
            // 매개변수 0 : 제목입니다
            // 매개변수 끝

            MemoData newMemo;
            string Title;


            if(parameter == "")
            {
                Title = "No Title";
            }
            else
            {
                Title = parameter;
            }

            // 이름을 가지고 파일을 만들어봅니다. 만약 파일이 존재한다면 대체 이름을 만들어 다시 시도합니다.
            FileInfo fileInfo;
            int whileAttempt = 0;
            do
            {
                if (whileAttempt > 255)
                {
                    throw new CannotMakeAlternativeNameException("다른 이름을 선택하세요");
                }
                fileInfo = new FileInfo(string.Format($"{directoryPath}{Title}{fileType}"));

                if(fileInfo.Exists) Title = String.Format($"{Title}_"); // 대체 이름을 만듭니다.
                whileAttempt++;
            }
            while (fileInfo.Exists);

            // 파일 만들기가 준비되었습니다.
            
            newMemo = new MemoData(lastId + 1, Title);
            FileStream makeStream = new FileStream(string.Format($"{directoryPath}{Title}{fileType}"), FileMode.CreateNew); // 파일이 이미 있다고 예외를 내뿜습니다.

            makeStream.Close();

            if(fileInfo.Exists == true)
            {
                Console.WriteLine($"파일이 성공적으로 만들어졌습니다. 파일 이름 : {Title}");
            }

            

            // 메모를 열어봅니다
            NextCommand.Enqueue(new CommandCaller(OpenMemo, parameter));
        }
        public class CannotMakeAlternativeNameException : Exception
        {
            public CannotMakeAlternativeNameException() : base()
            {

            }
            public CannotMakeAlternativeNameException(string _message) : base(_message)
            {

            }
        }

#warning 로직 구현 중입니다.
        // "!열기 [제목]"
        void OpenMemo(string parameter)
        {
            // 함수 설명
            // 문서의 내용을 보여줍니다.

            // 지역 변수
            int ShowLineSize = 20; // 한번에 보여줄 라인입니다.

            

            mode = ViewMode.Edit;


        }

#warning 테스트 하지 않은 함수입니다
        // "!페이지"
        void ShowPageList(string parameter)
        {
            // 비주얼 용으로 사용된 필드.
            int countPerPage = 10; // 한 페이지에 보여질 메모의 갯수
            int edgeBuffer = 3; // 가장 양 끝 페이지 포함 몇 페이지까지 보여주게 할 것인가
            int cursorBuffer = 5; // 현재 선택한 페이지(제외)의 앞 뒤로 몇 페이지까지 보여주게 할 것인가



            if (parameter == "")
            {

            }

            pageNum = int.Parse(parameter);

            Console.WriteLine("메모 목록");
            Console.WriteLine();
            Console.WriteLine("인덱스\t/제목");
            for (
                int index = countPerPage * pageNum;
                (index < countPerPage * (pageNum + 1)) && (index < memoList.Count);
                index++)
            {
                Console.WriteLine($"{index}\t/ {memoList[index].title}");
            }
            Console.WriteLine();
            
            // 시작 인덱스를 작성합니다.
            for(int i = 0; i < edgeBuffer; i++)
            {
                if(i >= (pageNum - cursorBuffer))
                {
                    // pageNum 근처이므로 출력하지 않습니다.
                    break;
                }
                Console.Write($"{i} ");

                // (n - cursorbuffer)까지 반복한다.
            }
            if (pageNum < 4)
            {
                Console.Write(" ...");
            }
            Console.WriteLine("도움이 필요하시면 다음의 명령어를 입력하세요 : !명령어");

            Console.WriteLine(); // 1 2 3 ... n-5 n-4 n-3 n-2 n-1 [n] n+1 n+2 n+3 n+4 n+5 ... m-2 m-1 m
            // n은 현재 선택한 페이지, m은 최대 페이지
            // 1부터 3까지
            // 
            
            








        }
#warning 테스트 하지 않은 함수입니다
        // "!다음"
        void ShowNextList(string parameter)
        {
            // 함수 설명
            // 다음 페이지를 출력합니다.
            //
            // 입력 A
            // 입력이 없으면
            // 출력 A
            // 곧장 바로 다음 페이지를 출력합니다
            //
            // 입력 B
            // 입력이 숫자이면
            // 출력 B
            // 해당하는 페이지만큼 이동합니다.


            if(parameter.Length == 0)
            {
                NextCommand.Enqueue(new CommandCaller(ShowPageList, (++pageNum).ToString()));
                return;
                // 다음 함수 사이에는 명령문 듣는 상태가 아닙니다
            }

            try
            {
                pageNum += int.Parse(parameter);
            }
            catch (FormatException)
            {
                Cry("숫자를 입력해주세요. 이 명령어는 정수만 받을 수 있습니다.");
                return;
            }
            catch (OverflowException)
            {
                Cry("입력해준 숫자가 너무 커요.");
            }
            catch (ArgumentNullException)
            {
                Cry("들어온 인자가 null 값입니다");
            }

            NextCommand.Enqueue(new CommandCaller(ShowPageList, pageNum.ToString()));
            // 다음 함수 사이에는 명령문 듣는 상태가 아닙니다.
        }
#warning 테스트 하지 않은 함수입니다
        // "!이전"
        void ShowPervList(string parameter)
        {

        }

        // 근데 이게 필요한지 명확하지 않음.
        public void ReceveCommand(string commandLine)
        {
            // 함수 설명
            // 명령어를 받고 그에 맞는 함수를 호출해줍니다.
            // 앞 내용이 유효한 인자인경우 적당히 쪼개줄 수 있습니다.
            // 명령어 스트링으로 추정되는 명령어 라인을 매개변수로 받는다
            // 명령어인지 체크한다
            // 명령어 신호를 덜어내고, 유효한 명령어 + 빈칸 ' '을 기준으로 명령어와 매개변수를 구분해낸다.
            // 맞는 함수를 리턴한다.

            // 시그너쳐를 통해 명령어인지 판정한다
            if(commandLine.StartsWith(commandSigniture) == false) // 명령어가 아닌 경우
            {
                Console.WriteLine("올바른 명령어가 아닙니다. !help 혹은 !도움 을 입력하세요");
                return;
            }

            // 명령어 부분을 제거한다
            commandLine = commandLine.Remove(0, commandSigniture.Length);

            // 명령어 / 공백 / 매개변수로 구분한다.
            int stringIndex = commandLine.IndexOf(' ');
            string command = stringIndex != -1  ? commandLine.Remove(stringIndex)       : commandLine;
            string argument = stringIndex != -1 ? commandLine.Remove(0, stringIndex+1)    : "";

            Console.WriteLine(command);
            Console.WriteLine(argument);
            // 받은 스트링에 기반하여 명령어를 실행한다. 현재 씬 상태도 고려해야 한다.
            switch (mode)
            {
                case ViewMode.Main:
                    try
                    {
                        mainSceneCommands[command](argument);
                    }
                    catch(KeyNotFoundException)
                    {
                        Cry("알 수 없는 명령어예요.");
                    }
                    break;
                case ViewMode.Edit:
                    try
                    {
                        noteSceneCommands[command](argument);
                    }
                    catch (KeyNotFoundException)
                    {
                        Cry("알 수 없는 명령어예요.");
                    }
                    break;
                default:
                    Cry("알 수 없는 ViewMode가 설정되었습니다.");
                    //return null;
                    Kill();
                    break;
            }




        }

        #region 심연의 코드
        void Cry(string message, CryMode mode = CryMode.User, [CallerLineNumber] int line = 0, [CallerMemberName] string caller = "")
        {
            Console.WriteLine($"ERROR_{caller} at line {line}\t: {message}");
        }
        #endregion

        // 열거형
        enum ViewMode // 현재 플레이어가 보고 있는 씬의 이름을 나타냅니다.
        {
            Main    = 0,
            Edit    = 1
        }
        enum CryMode // 어떤 유저에게 오류 메시지가 보여질지 결정합니다
        {
            User    = 0,
            Dev     = 1
        }

    }

    [Serializable]
    class MemoData
    {
        // 정의
        // 한 메모지에 들어가는 내용들을 정의합니다.

        // 설명
        // id값은 이 메모가 시간순으로 몇번째로 만들어졌는지 카운트합니다.
        // 타이틀은 MainScene에 보이는 메모의 제목입니다.
        // content는 메모의 내용입니다.

        // 생성자
        public MemoData()
        {
            int id = -1;
            title = "No Title";
            content = new List<string>();
        }
        public MemoData(int id_) : this()
        {
            id = id_;
        }
        public MemoData(int id_, string title_) : this(id_)
        {
            title = title_;
        }

        // 필드
        public int id;
        public string title;
        public List<string> content;
    }

    class CommandCaller
    {
        // 정의
        // 매개변수를 가지고 있는 대리자입니다
        
        // 설명
        // commandcaller는 리스트 형식이 아니여야 하는 이유는
        //  바깥에서 List<CommandCaller> 형식으로 호출할 것이기 때문입니다.

        // 생성자
        public CommandCaller() { }
        public CommandCaller(Command command, params string[] strings) : this()
        {
            deleg = command;
            arguments = strings;
        }

        // 필드
        public Command deleg;
        public string[] arguments;
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
            self.Work();


            Console.ReadLine();
        }

        void ShowMain()
        {

        }
    }
}
