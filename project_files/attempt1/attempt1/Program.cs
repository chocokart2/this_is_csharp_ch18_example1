using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO
// 라인 수정하는 함수 만들기
// 임시 파일 로직 구현
// >> 어떻게 진짜 파일과 임시 파일을 구분할 것인가
// >> 어떻게 임시 파일을 진짜 파일에 뒤집어쓸 것인가


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
            mainSceneCommands.Add("열기", new Command(OpenMemo));
            mainSceneCommands.Add("응답", new Command(RespondCommand));
            mainSceneCommands.Add("이전", new Command(ShowPreviousList));
            mainSceneCommands.Add("삭제", new Command(DeleteMemo));
            mainSceneCommands.Add("생성", new Command(MakeMemo));
            mainSceneCommands.Add("지우기", new Command(DeleteMemo));
            mainSceneCommands.Add("페이지", new Command(ShowPageList));
            noteSceneCommands = new Dictionary<string, Command>();
            noteSceneCommands.Add("뒤로", new Command(GoBackToMenu));
            noteSceneCommands.Add("메뉴", new Command(GoBackToMenu));
            noteSceneCommands.Add("메인", new Command(GoBackToMenu));
            noteSceneCommands.Add("저장", new Command(SaveMemoToFile));
            noteSceneCommands.Add("제목", new Command(ChangeMemoTitle));
            //noteSceneCommands.Add("!", new Command(WriteMemoNewLine));


            
            // 마무리 코드
            UpdateFileList();
        }
        // 필드
//public:
//private:
        List<MemoData> memoList;
        string[] fileNameList;        

        int pageNum; // 0부터 시작하는 페이지 번호입니다.
        int maxPageNum;
        int countPerPage = 3; // 한 페이지에 보여질 메모의 갯수
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
        bool hasMemoEdited = false; // 만약 객체 수정이 이루어졌다고 생각되면 true로 바뀝니다.
        string currentOpenedFileName; // 확장자 포함한 이름입니다.
        readonly string commandSigniture = "!";
        readonly string directoryPath = "memo_files\\";
        readonly string fileType = ".chokart"; // 확장자 파일입니다.
        readonly string fileTypeTemp = ".chotemp"; // 확장자 파일입니다.
        MemoData currentOpenedMemoObject;
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
            
            // 그 이후 파일명을 배열애 죄다 저장한다. -> 스트림에 저장하여 파일에 접근할 수 있습니다.
            FileInfo[] fileInfos = directoryInfo.GetFiles(); // fileInfo.Name을 이용하여 파일에 접근하기

            FileStream openerStream;
            memoList = new List<MemoData>();
            for (int index = 0; index < fileInfos.Length; index++) // 받은 파일 목록을 전부 조사한다.
            {
                openerStream = new FileStream($"{directoryPath}{fileInfos[index].Name}", FileMode.Open);

                BinaryFormatter deserializer = new BinaryFormatter();


                try
                {
                    MemoData recevedObject
                        = (MemoData)deserializer.Deserialize(openerStream);
                    memoList.Add(recevedObject);
                }
                catch (ArgumentNullException ex)
                {
                    Console.WriteLine(ex);
                    Cry("오프너 스트림이 널 값이에요.");
                }
                catch(SerializationException ex)
                {
                    Console.WriteLine(ex);
                    Cry("직렬화하는데 문제가 생겼어요.");
                }
                catch(SecurityException ex)
                {
                    Console.WriteLine(ex);
                    Cry("보안 오류가 감지되었어요.");
                }



                openerStream.Close();
            }

            maxPageNum = memoList.Count % countPerPage == 0 ? memoList.Count / countPerPage : memoList.Count / countPerPage + 1;
        }
        public void Work()
        {
            // 함수 설명
            // 명령어를 듣고 명령에 해당하는 함수를 호출합니다.
            string recevedString;
            // 호출할 함수가 없음 -> 명령어를 듣는 상태로 변경

            // 초기 파트

            ShowPageList("0");

            for (; ; )
            {
                if (workLoop == false) break;


                Console.Write("명령어 입력>");
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

        #region ViewMode.Main 용 함수
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
            BinaryFormatter serializer = new BinaryFormatter();

            serializer.Serialize(makeStream, newMemo);
            // 파일 체크
            fileInfo.Refresh();
            if(fileInfo.Exists == true)
            {
                Console.WriteLine($"파일이 성공적으로 만들어졌습니다. 파일 이름 : {Title}");
            }



            makeStream.Close();



            UpdateFileList();

            // 메모를 열어봅니다
            NextCommand.Enqueue(new CommandCaller(OpenMemo, Title));
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
        void OpenMemo(string parameter)
        {
            // >>함수 설명
            // 문서의 내용을 보여줍니다.
            // 입력 값 A : 이름 값
            // 입력 겂 B : 인덱스 값
            // 출력 값 A / B : 파일 출력

            // >> 지역 변수 준비
            int ShowLineSize = 20; // 한번에 보여줄 라인입니다.
            int index; // 인덱스 값으로 아규먼트가 들어온 경우.
            FileStream fileStream;
            hasMemoEdited = false;

            // >> 예외 처리하기
            while (parameter.EndsWith("\\"))
            {
                parameter = parameter.Remove(parameter.Length - 1, 1);
            }
            // >> 입력받은 값을 기반으로 파일 특정하기
            try
            {
                fileStream = new FileStream($"{directoryPath}{parameter}{fileType}", FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                MemoData recvMemo = (MemoData)formatter.Deserialize(fileStream);

                currentOpenedFileName = $"{parameter}{fileType}";
                currentOpenedMemoObject = recvMemo;

                fileStream.Close();
                // >> 상태 변경
                this.mode = ViewMode.Edit;

                NextCommand.Enqueue(new CommandCaller(ShowCurrentMemoObjectContent));
            }
            catch(FileNotFoundException)
            {
                if (parameter.EndsWith(fileType))
                {
                    Cry($"입력한 파일이 존재하지 않아요, 파일 이름에 확장자 {fileType}을 제거하고 입력해보세요.");
                }
                else
                {
                    Cry("입력한 파일이 존재하지 않아요");
                }
            }




        }// "!열기 [제목]"
#warning 테스트 하지 않은 함수입니다
        void ShowPageList(string parameter)
        {
            // + 폴더 정렬은 업데이트에서 합니다. 여기서는 보여주기만 해요.
            // 프론트 엔드용 변수
            //int countPerPage = 10; // 한 페이지에 보여질 메모의 갯수
            int startNum = 1;

            int edgeBuffer = 3; // 가장 양 끝 페이지 포함 몇 페이지까지 보여주게 할 것인가
            int cursorBuffer = 5; // 현재 선택한 페이지(제외)의 앞 뒤로 몇 페이지까지 보여주게 할 것인가

            // 현재 페이지가 몇 페이지인가를 결정합니다.
            if (parameter == "")
            {
                pageNum = 0;
            }
            else
            {
                try
                {
                    pageNum = int.Parse(parameter) - 1;
                    if (pageNum < 0) pageNum = 0;
                    if (pageNum >= maxPageNum) pageNum = maxPageNum - 1;
                }
                catch (FormatException ex)
                {
                    Cry("숫자를 입력해주세요");
                    return;
                }
                catch (OverflowException ex)
                {
                    Cry("입력한 숫자가 너무 커요");
                    return;
                }

            }
            
            // 현재 목록의 페이지를 보여줍니다.
            Console.WriteLine("메모 목록");
            Console.WriteLine();
            Console.WriteLine("인덱스\t| 제목");
            for (
                int index = countPerPage * pageNum;
                (index < countPerPage * (pageNum + 1)) && (index < memoList.Count);
                index++)
            {
                Console.WriteLine($"{index}\t| {memoList[index].title}");
            }
            Console.WriteLine();

            // 1 2 3 ... n-5 n-4 n-3 n-2 n-1 [n] n+1 n+2 n+3 n+4 n+5 ... m-2 m-1 m
            // n은 현재 선택한 페이지, m은 최대 페이지
            // 1부터 3까지

            // 시작 인덱스를 작성합니다.
            for (int i = startNum; i < startNum + edgeBuffer; i++)
            {
                if(i >= (pageNum - cursorBuffer + 1))
                {
                    // pageNum 근처이므로 출력하지 않습니다.
                    break;
                }
                Console.Write($" {i}");

                // (n - cursorbuffer)까지 반복한다.
            }
            if (pageNum - cursorBuffer + 1 > startNum + edgeBuffer)
            {
                Console.Write(" ...");
            }
            for(int i = Math.Max(pageNum-cursorBuffer, 0) ; i < pageNum; i++)
            {

                Console.Write($" {i + 1}");
            }
            Console.Write($" [{pageNum + 1}]");

            for (int i = pageNum + 1; i <= Math.Min(cursorBuffer + pageNum, maxPageNum - edgeBuffer - 1); i++)
            {

                Console.Write($" {i + startNum}");
            }
            if(pageNum + cursorBuffer + startNum < maxPageNum - edgeBuffer)
            {
                Console.Write(" ...");
            }
            for (int i = Math.Max(pageNum + startNum, maxPageNum - edgeBuffer); i < maxPageNum; i++)
            {
                // pageNum이 오른쪽 끝 - 버퍼보다 더 큰 경우에는 출력하지 않습니다.
                Console.Write($" {i + startNum}");
            }


            Console.WriteLine();
            Console.WriteLine("도움이 필요하시면 다음의 명령어를 입력하세요 : !명령어");

            Console.WriteLine();
            // 
            
            Console.WriteLine($"maxPageNum의 값 : {maxPageNum}");








        }// "!페이지"
#warning 테스트 하지 않은 함수입니다
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
                pageNum++;
                NextCommand.Enqueue(new CommandCaller(ShowPageList, (pageNum + 1).ToString()));
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

            NextCommand.Enqueue(new CommandCaller(ShowPageList, (pageNum + 1).ToString()));
            // 다음 함수 사이에는 명령문 듣는 상태가 아닙니다.
        }// "!다음"
#warning 테스트 하지 않은 함수입니다
        void ShowPreviousList(string parameter)
        {
            if (parameter.Length == 0)
            {
                NextCommand.Enqueue(new CommandCaller(ShowPageList, (--pageNum + 1).ToString()));
                return;
                // 다음 함수 사이에는 명령문 듣는 상태가 아닙니다
            }

            try
            {
                pageNum -= int.Parse(parameter);
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

            NextCommand.Enqueue(new CommandCaller(ShowPageList, (pageNum + 1).ToString()));
        }// "!이전"
#warning 테스트 지수 : 가라
        void DeleteMemo(string parameter)
        {
            // 함수 설명
            // 이 파일을 삭제합니다
            FileInfo fileInfo = new FileInfo($"{directoryPath}{parameter}{fileType}");
            if (fileInfo.Exists)
            {
                try
                {
                    fileInfo.Delete();
                    fileInfo.Refresh();
                    if (fileInfo.Exists == false)
                    {
                        Say("파일이 성공적으로 삭제되었습니다");
                    }
                    else
                    {
                        Cry("삭제 실패!");
                    }
                }
                catch (Exception e)
                {
                    Cry(e.Message);
                }
            }
            else
            {
                Say("파일이 존재하지 않아 아무것도 하지 않았어요.");
            }
            UpdateFileList();
            NextCommand.Enqueue(new CommandCaller(ShowPageList));
        }// "!지우기 !삭제"
        #endregion
        // 노트 커멘드
        #region ViewMode.Edit용 함수

        void GoBackToMenu(string parameter)
        {
            currentOpenedFileName = "";
            mode = ViewMode.Main;
            NextCommand.Enqueue(new CommandCaller(ShowPageList));
        }// "!뒤로 !메뉴 !메인"
        /// <summary>
        /// 이 함수는 noteSceneCommands에 Value로 넣을 수 없는 함수입니다. 만약 그러길 원한다면 함수륾 만드세요.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="parameter"></param>
        void WriteMemoLine(int line, string parameter)
        {
            hasMemoEdited = true;
            try
            {
                currentOpenedMemoObject.content[line] = parameter;
                NextCommand.Enqueue(new CommandCaller(ShowCurrentMemoObjectContent));
            }
            catch (ArgumentOutOfRangeException)
            {
                if(line >= 0)
                {
                    WriteMemoNewLine(parameter);
                }
                else
                {
                    Cry("라인 값은 음수가 될 수 없어요.");
                }
            }



        }
        void ChangeMemoTitle(string parameter)
        {
            hasMemoEdited = true;

            currentOpenedMemoObject.title = parameter;

            NextCommand.Enqueue(new CommandCaller(ShowCurrentMemoObjectContent));

        } // !제목
#warning 만약 파일 이름에 넣을 수 없는 문자가 들어가는경우 이를 필터해야 합니다
        void SaveMemoToFile(string parameter)
        {
            // 함수 설명
            // 현재 선택 중인 함수의 객체를 파일에 저장합니다
            // 만약 입력값이 있다면 해당 입력값으로 제목을 지정해 파일을 찾거나 새로 만들어 저장합니다.

            // 준비!
            string targetPath;
            string targetTitle;
            if (currentOpenedFileName == null ||
                currentOpenedMemoObject == null)
            {
                Cry("현재 수정중인 메모 객체가 존재하지 않습니다.");
            }


            // >> 어느 파일에 저장할 것인가? // 어떤 파일의 이름으로 저장될 것인가?
            // 만약 아무 입력이 없다면 지금 조작중인 객체를 저장합니다.
            if(parameter == "") // 새로운 제목으로 저장하지 않음
            {
                targetPath = $"{directoryPath}{currentOpenedFileName}";

                FileInfo fileInfo = new FileInfo(targetPath);


                // 제목을 바꾼 흔적이 있는지 조사
                if (currentOpenedFileName != currentOpenedMemoObject.title)
                {
#warning 근데 여기 브라켓 안에 targetPath에 할당된 값은 브라켓 바깥으로 나가면 휘발될까요?
                    targetPath = $"{directoryPath}{currentOpenedMemoObject.title}{fileType}";

                    // 제목을 바꿨으므로 MoveTo 함수를 호출합니다.
                    // 주소를 옮겼으므로 옮긴 주소로 타게팅을 다르게 합니다.
                    fileInfo.MoveTo(targetPath);
                    fileInfo = new FileInfo(targetPath);
                }

                FileStream fs = new FileStream(targetPath, FileMode.Open);
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(fs, currentOpenedMemoObject);
                currentOpenedFileName = $"{currentOpenedMemoObject.title}{fileType}";

                fs.Close();
            }
            else
            {
                if (!IsThatNameOkay(parameter))
                {
                    Cry("올바르지 않은 파일의 이름이예요.");
                    NextCommand.Enqueue(new CommandCaller(ShowCurrentMemoObjectContent));
                    return;
                }

                targetPath = $"{directoryPath}{parameter}{fileType}";
                FileStream fs = new FileStream(targetPath, FileMode.Create);
                BinaryFormatter serializer = new BinaryFormatter();

                currentOpenedMemoObject.title = parameter;
                serializer.Serialize(fs, currentOpenedMemoObject);

                fs.Close();
            }



            if(new FileInfo($"{directoryPath}{currentOpenedFileName}").Exists)
            {
                FileStream saveStream = new FileStream($"{directoryPath}{currentOpenedFileName}", FileMode.Open);

                saveStream.Close();
            }


            // 만약 currentOpenedFileName과 currentOpenedMemoObject.title의 내용이 다르다면
            hasMemoEdited = false;
            UpdateFileList();
            NextCommand.Enqueue(new CommandCaller(ShowCurrentMemoObjectContent));
        }
        void SaveMemoWithNewName()
        {
            // >> 함수 설명
        }

        void WriteMemoNewLine(string parameter)
        {
            hasMemoEdited = true;
            currentOpenedMemoObject.content.Add(parameter);
            NextCommand.Enqueue(new CommandCaller(ShowCurrentMemoObjectContent));
        }
        bool IsThatNameOkay(string parameter)
        {
            return
                !(parameter.Contains("/") ||
                parameter.Contains("\\") ||
                parameter.Contains(":") ||
                parameter.Contains("*") ||
                parameter.Contains("?") ||
                parameter.Contains("\"") ||
                parameter.Contains("<") ||
                parameter.Contains(">") ||
                parameter.Contains("|"));
        }


        void ShowCurrentMemoObjectContent(string parameter)
        {
            // >> 함수 설명
            // 입력값 : 아무래도 상관 없어
            // 출력값 : 현재 메모의 내용을 콘솔로 출력한다.
            Console.WriteLine();
            Console.WriteLine("파일 내용");
            Console.WriteLine();
            Console.Write($"제목\t| {currentOpenedMemoObject.title}");
            if (hasMemoEdited) Console.Write("(수정됨)");
            Console.WriteLine();
            Console.WriteLine($"라인\t| 내용");

            for (int memoLine = 0; memoLine < currentOpenedMemoObject.content.Count; memoLine++)
            {
                Console.WriteLine($"{memoLine}\t| {currentOpenedMemoObject.content[memoLine]}");
            }
            if(currentOpenedMemoObject.content.Count == 0)
            {
                Console.WriteLine("\t| === 내용 없음 ===");
            }
        }

        #endregion


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
                Say("올바른 명령어가 아닙니다. !help 혹은 !도움 을 입력하세요");
                return;
            }

            // 명령어 부분을 제거한다
            commandLine = commandLine.Remove(0, commandSigniture.Length);

            // 명령어 / 공백 / 매개변수로 구분한다.
            int stringIndex = commandLine.IndexOf(' ');
            string command = stringIndex != -1  ? commandLine.Remove(stringIndex)       : commandLine;
            string argument = stringIndex != -1 ? commandLine.Remove(0, stringIndex+1)    : "";

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
                    // !<라인> <문자> 형식인 경우 이를 예외로 한다.
                    int memoLine = 0;
                    if(int.TryParse(command, out memoLine))
                    {
                        // 라인
                        WriteMemoLine(memoLine, argument);

                    }
                    else if (commandLine.StartsWith(commandSigniture))
                    {
                        WriteMemoNewLine(commandLine.Remove(0, commandSigniture.Length));
                    }
                    else
                    {
                        // 단순 명령 상태입니다.
                        try
                        {
                            noteSceneCommands[command](argument);
                        }
                        catch (KeyNotFoundException)
                        {
                            Cry("알 수 없는 명령어예요.");
                        }
                    }



                    break;
                default:
                    Cry("알 수 없는 ViewMode가 설정되었습니다.");
                    //return null;
                    Kill();
                    break;
            }
            for(int trycount = 0; trycount < 256; trycount++)
            {

                if(NextCommand.Count <= 0)
                {
                    // 대기중인 명령이 다 끝났습니다.
                    break;
                }
                else
                {
                    NextCommand.Dequeue().Run();
                }
                if(trycount == 255 && NextCommand.Count > 0)
                {
                    Cry("NextCommand에 존재하는 명령어가 제거되지 않습니다!");
                }
            }
        }

        #region 심연의 코드
        void Cry(string message, CryMode mode = CryMode.User, [CallerLineNumber] int line = 0, [CallerMemberName] string caller = "")
        {
            Console.WriteLine($"ERROR_{caller} at line {line}\t: {message}");
        }
        void Say(string message)
        {
            Console.WriteLine($"프로그램 : {message}");
        }
        #endregion
        #region 마리아나 해구 급으로 숨겨진 테스트용 함수
        void MakeDummyFiles()
        {
            for(int count = 0; count < 30; count++)
            {
                MakeMemo($"임의의_파일_{count.ToString()}");
            }

        }


        #endregion

        // 열거형
        #region MyRegion
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
        #endregion


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
        public CommandCaller(Command command) : this()
        {
            deleg = command;
            argument = "";
        }
        public CommandCaller(Command command, string recvArgument) : this(command)
        {
            deleg = command;
            argument = recvArgument;
        }

        // 필드
        public Command deleg;
        public string argument;

        // 함수
        public void Run()
        {
            deleg(argument);
        }
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
