﻿using StudentManagement.Commands;
using StudentManagement.Objects;
using StudentManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using static StudentManagement.Services.LoginServices;
using static StudentManagement.ViewModels.AdminSubjectClassViewModel;

namespace StudentManagement.ViewModels
{
    public class AdminSubjectClassRightSideBarViewModel : BaseViewModel
    {
        #region properties
        private static AdminSubjectClassRightSideBarViewModel s_instance;
        public static AdminSubjectClassRightSideBarViewModel Instance
        {
            get => s_instance ?? (s_instance = new AdminSubjectClassRightSideBarViewModel());

            private set => s_instance = value;
        }

        private object _rightSideBarItemViewModel;

        public object RightSideBarItemViewModel
        {
            get { return _rightSideBarItemViewModel; }
            set
            {
                _rightSideBarItemViewModel = value;
                OnPropertyChanged();
            }
        }

        private object _adminSubjectClassRightSideBarItemViewModel;

        private object _emptyStateRightSideBarViewModel;
        #endregion

        #region icommand

        public ICommand ShowCardInfo { get => _showCardInfo; set => _showCardInfo = value; }

        private ICommand _showCardInfo;
        public ICommand EditSubjectClassCardInfo { get => _editSubjectClassCardInfo; set => _editSubjectClassCardInfo = value; }

        private ICommand _editSubjectClassCardInfo;
        public ICommand DeleteSubjectClassCardInfo { get => _deleteSubjectClassCardInfo; set => _deleteSubjectClassCardInfo = value; }

        private ICommand _deleteSubjectClassCardInfo;

        public ICommand CreateSubjectClassCardInfo { get => _createSubjectClassCardInfo; set => _createSubjectClassCardInfo = value; }

        private ICommand _createSubjectClassCardInfo;
        #endregion

        public AdminSubjectClassRightSideBarViewModel()
        {
            Instance = this;
            LoginServices.UpdateCurrentUser += FreeRightSideBar;

            InitRightSideBarItemViewModel();

            ShowCardInfo = new RelayCommand<UserControl>((p) => { return true; }, (p) => ShowCardInfoByCardDataContext(p));
            EditSubjectClassCardInfo = new RelayCommand<object>((p) => { return true; }, (p) => EditSubjectClassCardByCardFunction(p));
            DeleteSubjectClassCardInfo = new RelayCommand<object>((p) => { return true; }, (p) => DeleteSubjectClassCardByCardFunction(p));
            CreateSubjectClassCardInfo = new RelayCommand<object>((p) => { return true; }, (p) => CreateSubjectClassCardByCardFunction());
        }

        #region methods
        public void InitRightSideBarItemViewModel()
        {
            _adminSubjectClassRightSideBarItemViewModel = new AdminSubjectClassRightSideBarItemViewModel();
            _emptyStateRightSideBarViewModel = new EmptyStateRightSideBarViewModel();

            RightSideBarItemViewModel = _emptyStateRightSideBarViewModel;
        }
        public void ShowCardInfoByCardDataContext(UserControl p)
        {
            SubjectClassCard card = p.DataContext as SubjectClassCard;

            _adminSubjectClassRightSideBarItemViewModel = new AdminSubjectClassRightSideBarItemViewModel(card);

            RightSideBarItemViewModel = _adminSubjectClassRightSideBarItemViewModel;
        }

        public void EditSubjectClassCardByCardFunction(object p)
        {
            SubjectClassCard card = p as SubjectClassCard;

            _adminSubjectClassRightSideBarItemViewModel = new AdminSubjectClassRightSideBarItemEditViewModel(card);

            RightSideBarItemViewModel = _adminSubjectClassRightSideBarItemViewModel;
        }

        public void CreateSubjectClassCardByCardFunction()
        {
            SubjectClassCard card = new SubjectClassCard();

            _adminSubjectClassRightSideBarItemViewModel = new AdminSubjectClassRightSideBarItemEditViewModel(card, isCreatedNew: true);

            RightSideBarItemViewModel = _adminSubjectClassRightSideBarItemViewModel;
        }

        public void DeleteSubjectClassCardByCardFunction(object p)
        {
            SubjectClassCard card = p as SubjectClassCard;

            if (MyMessageBox.Show($"Bạn thực sự muốn xóa lớp {card?.Code}? Xóa lớp học sẽ xóa toàn bộ dữ liệu trong lớp học!!!", "Thông báo", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                bool success = SubjectClassServices.Instance.RemoveSubjectClassFromDatabaseBySubjectClassId(card.Id);

                if (success)
                {
                    SubjectClassCards.Remove(card);
                    StoredSubjectClassCards.Remove(card);
                    MyMessageBox.Show($"Xóa lớp {card.Code} thành công");
                }
                else
                {
                    MyMessageBox.Show("Có lỗi kết nối đến cơ sở dữ liệu, vui lòng thử lại sau");
                }
                RightSideBarItemViewModel = _emptyStateRightSideBarViewModel;
            }
        }
        #endregion


        #region eventhandler
        private void FreeRightSideBar(object sender, LoginEvent e)
        {
            _rightSideBarItemViewModel = _emptyStateRightSideBarViewModel;
        }
        #endregion
    }
}
