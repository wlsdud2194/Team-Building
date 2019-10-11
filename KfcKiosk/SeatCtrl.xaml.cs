﻿using Kfc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KfcKiosk
{
    public class SeatArgs : EventArgs
    {

    }

    public partial class SeatCtrl : UserControl
    {
        delegate void Work();
        public delegate void OnSeatEventHandler(object sender, SeatArgs args);
        public event OnSeatEventHandler SeatEvent;


        private Seat selectedSeat { get; set; }

        public SeatCtrl()
        {
            InitializeComponent();
            Dispatcher.Invoke(DispatcherPriority.Normal, new Work(PutCurrentTime));
            this.Loaded += SeatCtrl_Loaded;
            paymentCtrl.PayEvent += PaymentCtrl_OnPaymentEvent;
        }

        private void SeatCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            lvFloor.ItemsSource = App.floorData.lstFloor;
        }

        private void PaymentCtrl_OnPaymentEvent(object sender, PayArgs args)
        {
            SeatPayPage_Toggle();
            OnOrderInfoHandler(args.selectedSeat.OrderInfo);
        }

        private void OnOrderInfoHandler(string orderInfo) // 주문 목록 업데이트
        {
            seatOrderInfo.Content = orderInfo;
        }

        // 현재 시간
        private void PutCurrentTime()
        {
            currentTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, new Work(PutCurrentTime));
        }

        // Floor Selection changed
        private void LvFloor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Floor floor = (lvFloor.SelectedItem as Floor);

            UpdateSeat(floor.Idx);
        }

        // 층을 선택했을 때 floorIdx를 인자로 받아 업데이트
        private void UpdateSeat(int floorIdx)
        {
            List<Seat> SelectedSeat = App.seatData.FilterSeat(floorIdx);
            lvSeat.ItemsSource = SelectedSeat;
        }

        // Seat Selection changed
        private void LvSeat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvSeat.SelectedIndex < 0)
            {
                lvSeat.SelectedIndex = 0;
            }

            Seat seat = lvSeat.SelectedItem as Seat;
            selectedSeat = seat;

            UpdateInfo(seat);
        }

        private void UpdateInfo(Seat seat)
        {
            seatId.Text = seat.Id;
            seatOrderInfo.Content = seat.OrderInfo;

            seatCheckBtn.Visibility = Visibility.Visible;
            seatFoodBtn.Visibility = Visibility.Visible;
        }

        private void SeatCheckBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSeat.lstFood.Count == 0)
            {
                MessageBox.Show("주문한 메뉴가 없습니다!", "WARNING");
                return;
            }

            paymentSeatId.Text = selectedSeat.Id;

            paymentAlert.Visibility = Visibility.Visible;
        }

        private void Check_Payment_Click(object sender, RoutedEventArgs e)
        {

            // 결제된 메뉴에 추가
            App.statData.AppendPaidFoods(selectedSeat.lstFood);

            // selectedSeat의 메뉴 초기화
            App.seatData.ClearSeat(selectedSeat.Id);

            paymentAlert.Visibility = Visibility.Collapsed;

            MessageBox.Show("결제 되었습니다", "SUCCESS");
        }
        
        private void SeatPayBtn_Click(object sender, RoutedEventArgs e)
        {
            SeatPayPage_Toggle();
        }

        private void SeatPayPage_Toggle()
        {
            if (seatCtrl.Visibility == Visibility.Visible)
            {
                paymentCtrl.SelectedSeat = selectedSeat;
                seatCtrl.Visibility = Visibility.Collapsed;
                paymentCtrl.Visibility = Visibility.Visible;
                paymentCtrl.LoadOrderList();
            }
            else
            {
                seatCtrl.Visibility = Visibility.Visible;
                paymentCtrl.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnStatView_Click(object sender, RoutedEventArgs e)
        {
            SeatArgs args = new SeatArgs();


            if (SeatEvent != null)
            {
                SeatEvent(this, args);
            }
        }

    }
}