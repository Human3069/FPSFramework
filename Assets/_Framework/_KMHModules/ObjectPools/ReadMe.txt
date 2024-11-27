1. BaseObjectPoolManager : ObjectPoolManager와 여러개의 ObjectPoolHandler 추상클래스가 포함되어있습니다.
	- ObjectPoolManager : 딕셔너리로 여러개의 ObjectPoolHandler를 관리합니다.
	- ObjectPoolHandler : 하나의 오브젝트를 Queue로 관리합니다.

2. BaseObjectPoolController : 하나의 ObjectPoolController 추상클래스가 포함되어있습니다.
	- ObjectPoolController : 하나의 오브젝트를 Queue로 관리합니다.

BaseObjectPoolManager 와 BaseObjectPoolController의 차이점은 Queue의 갯수에 있습니다.
BaseObjectPoolManager 는 여러개의 Queue를 관리하지만 BaseObjectPoolController 는 그렇지 않습니다.
따라서 BaseObjectPoolController는 SoundManager 및 UIManager와 같은 다른 클래스에 의존하도록 설계되었습니다.