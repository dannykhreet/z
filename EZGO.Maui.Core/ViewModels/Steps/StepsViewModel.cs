using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Steps;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Shared;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class StepsViewModel : BaseViewModel
    {
        private readonly ITasksService _taskService;
        private const int collectionCount = 2;

        private Tuple<StepModel, StepModel> selectedTuple;
        public Tuple<StepModel, StepModel> SelectedTuple
        {
            get { return selectedTuple; }
            set
            {
                selectedTuple = value;
                SetTitle();
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Tuple<StepModel, StepModel>> taskTuples;

        public ObservableCollection<Tuple<StepModel, StepModel>> TaskTuples
        {
            get => taskTuples;
            set
            {
                taskTuples = value;

                OnPropertyChanged();
            }
        }

        private int currentIndex;
        public int CurrentIndex
        {
            get => currentIndex;
            set
            {
                currentIndex = value;

                OnPropertyChanged();
            }
        }

        private string name;

        public string Name
        {
            get => name;
            set
            {
                name = value;

                OnPropertyChanged();
            }
        }

        private List<StepModel> steps;

        public List<StepModel> Steps
        {
            get => steps;
            set
            {
                steps = value;

                OnPropertyChanged();
            }
        }

        public ICommand DetailCommand => new Command<StepModel>(obj =>
        {
            ExecuteLoadingAction(async () => await NavigateToDetailAsync(obj));
        }, CanExecuteCommands);

        /// <summary>
        /// Initializes a new instance of the <see cref="StepsViewModel"/> class.
        /// </summary>
        public StepsViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            ITasksService tasksService) : base(navigationService, userService, messageService, actionsService)
        {
            _taskService = tasksService;
        }

        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public override async Task Init()
        {
            InitializeSteps();

            await base.Init();
        }

        private void InitializeSteps()
        {
            if (Steps == null) return;

            int i = 1;
            Steps.ForEach(x => { x.Index = i; i++; });

            var count = (Steps.Count + (collectionCount - 1)) / collectionCount;
            int i2 = 0;
            var tList = new List<Tuple<StepModel, StepModel>>();

            for (int i3 = 0; i3 < count; i3++)
            {
                var pair = Steps.Skip(i2).Take(collectionCount).ToList();
                if (pair.Any())
                {
                    // add as many items in tuple as collectionCount
                    tList.Add(Tuple.Create(pair.ElementAtOrDefault(0), pair.ElementAtOrDefault(1)));
                    i2 += collectionCount;
                }
            }

            TaskTuples = new ObservableCollection<Tuple<StepModel, StepModel>>(tList);

            OnPropertyChanged(nameof(CurrentIndex));
            SelectedTuple = TaskTuples[CurrentIndex];
        }

        private void SetTitle()
        {
            string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskStepsPageNumberText);

            if (TaskTuples != null)
                Title = string.Format(result.ReplaceLanguageVariablesCumulative(), (CurrentIndex + 1), TaskTuples.Count);
        }

        private async Task NavigateToDetailAsync(object obj)
        {
            if (obj is StepModel step)
            {
                using var scope = App.Container.CreateScope();
                var itemsDetailViewModel = scope.ServiceProvider.GetService<ItemsDetailViewModel>();
                itemsDetailViewModel.Items = new List<Interfaces.Utils.IDetailItem>(Steps);
                itemsDetailViewModel.SelectedItem = step;
                itemsDetailViewModel.SenderClassName = nameof(StepsViewModel);
                await NavigationService.NavigateAsync(viewModel: itemsDetailViewModel);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _taskService.Dispose();
            base.Dispose(disposing);
        }
    }
}
