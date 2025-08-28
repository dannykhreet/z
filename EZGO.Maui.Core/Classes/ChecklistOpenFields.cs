using Autofac;
using EZGO.Maui.Core.Classes.Stages;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Local;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using System.Text;

namespace EZGO.Maui.Core.Classes
{
    public class ChecklistOpenFields : NotifyPropertyChanged, IDisposable
    {
        #region Properties

        private bool _fieldsReadonly = false;
        private Tuple<double, double> propertyListHeight = new Tuple<double, double>(0, 0);

        public Tuple<double, double> PropertyListHeight
        {
            get
            {
                double item1 = propertyListHeight.Item1 == 0 ? 0 : propertyListHeight.Item1; // can't be 0 - crashes
                double item2 = propertyListHeight.Item2 == 0 ? 0 : propertyListHeight.Item2; // same
                return new Tuple<double, double>(item1, item2);

            }
            set => propertyListHeight = value;
        }
        public List<UserValuesPropertyModel> PropertyList { get; set; }

        public List<TemplatePropertyModel> TemplatePropertyModels { get; set; }

        public bool PropertySaved { get; set; } = false;

        public bool TasksDone { get; private set; }

        public List<BasicTaskTemplateModel> TaskTemplates { get; set; }

        private List<BasicTaskTemplateModel> ChangedTasks { get; set; } = new List<BasicTaskTemplateModel>();

        public int ChecklistTemplateId { get; set; }

        public DateTime StartedAt { get; set; }

        public StagesControl StagesControl { get; set; }

        public bool AnyTaskChanges { get; set; }

        private bool AnyOpenFieldsChanges { get; set; }

        private bool AnyPropertiesChanges { get; set; }

        private bool AnySignChanges { get; set; }

        public bool AnyLocalChanges => AnyTaskChanges || AnyOpenFieldsChanges || AnyPropertiesChanges || AnySignChanges;

        private bool isSyncing;
        public bool IsSyncing
        {
            get => isSyncing;
            set
            {
                isSyncing = value;
                CalculateTasksDone();
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructors
        public int FieldHeight { get; set; } = 61;

        public ChecklistOpenFields()
        {
            MessagingCenter.Subscribe<UserValuesPropertyModel>(this, Constants.ValueChanged, (sender) =>
            {
                CalculateTasksDone();
                AnyOpenFieldsChanges = true;
            });

            StartedAt = DateTime.UtcNow;
        }

        public ChecklistOpenFields(bool fieldsReadonly, int fieldHeight)
        {
            _fieldsReadonly = fieldsReadonly;
            FieldHeight = fieldHeight;
        }

        public ChecklistOpenFields(IOpenTextFields openTextFields) : this()
        {
            _fieldsReadonly = true;
            SetPropertyValues(openTextFields);
        }

        public ChecklistOpenFields(List<BasicTaskTemplateModel> taskTemplates, int checklistTemplateId) : this()
        {
            TaskTemplates = taskTemplates;
            ChecklistTemplateId = checklistTemplateId;

            if (!TaskTemplates.IsNullOrEmpty())
                CalculateTasksDone();
        }

        #endregion

        #region Public Methods

        public void SetAnyProperiesChanged(bool anyPropertiesChanges)
        {
            AnyPropertiesChanges = anyPropertiesChanges;
        }

        public void AddChangedTask(BasicTaskTemplateModel basicTaskTemplateModel, bool resetSignatures = true)
        {
            if (basicTaskTemplateModel == null)
                return;

            var isSelectedTaskInChanged = ChangedTasks.Contains(basicTaskTemplateModel);
            if (!isSelectedTaskInChanged)
            {
                ChangedTasks.Add(basicTaskTemplateModel);
                AnyTaskChanges = true;
            }
            if (resetSignatures)
            {
                StagesControl?.SignatureChanged(basicTaskTemplateModel);
                StagesControl?.TaskChanged(basicTaskTemplateModel);
            }
        }

        public void AddChangedStageSign()
        {
            AnySignChanges = true;
        }

        public List<BasicTaskTemplateModel> GetChangedTasks()
        {
            // Set modified properties 
            foreach (var item in ChangedTasks)
            {
                item.PropertyValues = new List<Api.Models.PropertyValue.PropertyUserValue>(item.ModifiedPropertyValues);
            }
            return ChangedTasks;
        }

        public void ClearChangedTasks()
        {
            foreach (var item in ChangedTasks)
            {
                item?.ModifiedPropertyValues?.Clear();
            }

            ChangedTasks?.Clear();
            AnyTaskChanges = false;
            AnySignChanges = false;
            AnyOpenFieldsChanges = false;
            AnyPropertiesChanges = false;
        }

        public async Task AdaptChanges(IOpenFieldLocalManager openFieldLocalManager)
        {
            if (TaskTemplates.IsNullOrEmpty()) return;

            if (await openFieldLocalManager.CheckIfLocalTemplateExistsAsync(ChecklistTemplateId).ConfigureAwait(false) ||
                TaskTemplates.Any(t => t.FilterStatus != Api.Models.Enumerations.TaskStatusEnum.Todo || t.Score != null))
            {
                PropertySaved = true;

                await SaveLocalChanges(openFieldLocalManager).ConfigureAwait(false);

                CalculateTasksDone();
            }
        }

        public void SetPropertyValues(IOpenTextFields taskTemplateModel, bool shouldClearPropertyIds = false)
        {
            if (shouldClearPropertyIds)
                taskTemplateModel.OpenFieldsPropertyUserValues.ForEach(x => x.Id = 0);

            if (taskTemplateModel != null && taskTemplateModel.OpenFieldsPropertyUserValues != null && taskTemplateModel.OpenFieldsProperties != null)
            {
                var openFieldsValues = taskTemplateModel.OpenFieldsPropertyUserValues;

                var templatePropertyModels = taskTemplateModel.OpenFieldsProperties.OrderBy(x => x.Index).ToList();

                if (openFieldsValues == null) return;

                UserValuesPropertyModel[] openFieldsResult = new UserValuesPropertyModel[templatePropertyModels.Count()];

                for (int i = 0; i < templatePropertyModels.Count; i++)
                {
                    var template = templatePropertyModels[i];
                    var openFieldValue = openFieldsValues.Where(x => x.TemplatePropertyId == template.Id).FirstOrDefault();

                    if (openFieldValue == null)
                    {
                        openFieldValue = CreateUserValue(template);
                    }
                    else
                    {
                        openFieldValue.IsReadonly = _fieldsReadonly;
                        openFieldValue.Title = template.TitleDisplay;
                        openFieldValue.IsRequired = template.IsRequired;
                    }

                    openFieldValue.Index = template.Index;
                    openFieldsResult[i] = openFieldValue;
                }

                taskTemplateModel.OpenFieldsPropertyUserValues = openFieldsResult.ToList();
                openFieldsValues = openFieldsValues.OrderBy(x => x.Index).ToList();

                SetProperties(openFieldsResult.ToList(), templatePropertyModels);
            }
            else if (taskTemplateModel != null && taskTemplateModel.OpenFieldsProperties != null)
            {
                var templates = taskTemplateModel.OpenFieldsProperties.OrderBy(x => x.Index).ToList();

                List<UserValuesPropertyModel> propertyList = CreateUserValues(templates);

                SetProperties(propertyList, templates);
            }
            else
            {
                SetProperties(new List<UserValuesPropertyModel>(), new List<TemplatePropertyModel>());
            }

            AnyOpenFieldsChanges = false;
        }

        private List<UserValuesPropertyModel> CreateUserValues(List<TemplatePropertyModel> templates)
        {
            var propertyList = new List<UserValuesPropertyModel>();

            templates.ForEach(x => propertyList.Add(CreateUserValue(x)));

            return propertyList;
        }

        private void SetProperties(List<UserValuesPropertyModel> openFieldsValues, List<TemplatePropertyModel> templatePropertyModels)
        {
            var openFieldsCount = openFieldsValues != null ? openFieldsValues.Count(x => x != null) : 0;
            PropertyListHeight = new Tuple<double, double>(0, 0);

            CalculateHeight(openFieldsCount);

            PropertyList = openFieldsValues;

            TemplatePropertyModels = templatePropertyModels;

            if (TaskTemplates != null)
                CalculateTasksDone();
        }

        private void CalculateHeight(int count)
        {
            PropertyListHeight = new Tuple<double, double>(count % 2 == 0 ? count / 2 * FieldHeight : ((count / 2) + 1) * FieldHeight, count);
        }

        public void CalculateTasksDone()
        {
            if (TaskTemplates?.Count > 0)
                TasksDone = !IsSyncing && TodoTasksCount() <= 0;
            else
                TasksDone = !IsSyncing;

            bool propertiesDone = true;

            if (PropertyList?.Count > 0)
                propertiesDone = !PropertyList.Any(x => x.IsRequired && x.GetFieldValue().IsNullOrWhiteSpace());

            if (StagesControl != null && StagesControl.HasStages)
                TasksDone = TasksDone && StagesControl.AreStagesSigned;

            TasksDone = TasksDone && propertiesDone;
        }

        public async Task SaveLocalChanges(IOpenFieldLocalManager openFieldLocalManager)
        {
            LocalTemplateModel model = new LocalTemplateModel
            {
                Id = ChecklistTemplateId,
                UserId = UserSettings.Id,
                OpenFieldsProperties = TemplatePropertyModels,
                OpenFieldsPropertyUserValues = PropertyList?.ToList(),
                TaskTemplates = TaskTemplates.Select(x => LocalTaskTemplateModel.FromBasic(x)).ToList(),
                StartedAt = StartedAt,
            };

            await openFieldLocalManager.AddOrUpdateLocalTemplateAsync(model).ConfigureAwait(false);
        }

        public void ShowInfoDialog(Page page)
        {
            var fields = PropertyList?.Where(x => x.IsRequired && x.GetFieldValue().IsNullOrEmpty()).ToList();
            if (!fields.IsNullOrEmpty())
            {
                StringBuilder message = new StringBuilder();
                fields.ForEach(x => message.Append($"{x.Title}\n"));
                page.DisplayAlert("Fill in required fields:", message.ToString(), "Ok");
            }
        }

        public void SendSyncingInProgressClosableWarning()
        {
            string text;
            text = TranslateExtension.GetValueFromDictionary(LanguageConstants.syncStatesViewSynsingMessage);
            using var scope = App.Container.CreateScope();
            var messagingCenter = scope.ServiceProvider.GetService<IMessageService>();
            messagingCenter.SendClosableWarning(text);
        }

        public IOpenTextFields CheckAndUpdateOpenFields(IOpenTextFields localChecklistTemplate, IOpenTextFields template)
        {
            if (template.OpenFieldsProperties != null)
                template.OpenFieldsPropertyUserValues = CreateUserValues(template.OpenFieldsProperties);

            if (localChecklistTemplate != null && template != null)
            {
                if (localChecklistTemplate.OpenFieldsPropertyUserValues != null)
                {
                    AssertCurrentValues(localChecklistTemplate.OpenFieldsPropertyUserValues, template);
                }
            }
            else if (PropertyList != null)
            {
                AssertCurrentValues(PropertyList, template);
            }

            return template;
        }

        public void Dispose()
        {
            //PropertyList = null;
            //TemplatePropertyModels = null;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<UserValuesPropertyModel>(this, Constants.ValueChanged);
            });
        }

        #endregion

        #region Private Methods

        private void AssertCurrentValues(List<UserValuesPropertyModel> openFieldsProperties, IOpenTextFields template)
        {
            template.OpenFieldsPropertyUserValues ??= new List<UserValuesPropertyModel>();

            // Build a lookup dictionary so we can update values in O(1)
            var templateMap = template.OpenFieldsPropertyUserValues
                .GroupBy(x => x.TemplatePropertyId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var currentValue in openFieldsProperties)
            {
                if (templateMap.TryGetValue(currentValue.TemplatePropertyId, out var itemToUpdate))
                {
                    itemToUpdate.TextInput = currentValue.TextInput;
                    itemToUpdate.Id = currentValue.Id;
                }
            }
        }

        private int TodoTasksCount() => TaskTemplates?.Count(x => x.FilterStatus == Api.Models.Enumerations.TaskStatusEnum.Todo && x.Score == null) ?? 1;

        private UserValuesPropertyModel CreateUserValue(TemplatePropertyModel template)
        {
            return new UserValuesPropertyModel
            {
                IsReadonly = _fieldsReadonly,
                TemplatePropertyId = template.Id,
                PropertyId = template.PropertyId,
                CompanyId = UserSettings.CompanyId,
                UserId = UserSettings.Id,
                ValueTypeEnum = template.ValueType,
                Title = template.TitleDisplay,
                IsRequired = template.IsRequired,
            };
        }

        #endregion
    }
}
