using System;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Desktop.Features.AllocationProfiles;
using CashflowSimulator.Desktop.Features.Analysis;
using CashflowSimulator.Desktop.Features.CashflowEvents;
using CashflowSimulator.Desktop.Features.CashflowStreams;
using CashflowSimulator.Desktop.Features.Eckdaten;
using CashflowSimulator.Desktop.Features.Korrelationen;
using CashflowSimulator.Desktop.Features.LifecyclePhases;
using CashflowSimulator.Desktop.Features.Marktdaten;
using CashflowSimulator.Desktop.Features.Meta;
using CashflowSimulator.Desktop.Features.Portfolio;
using CashflowSimulator.Desktop.Features.Settings;
using CashflowSimulator.Desktop.Features.StrategyProfiles;
using CashflowSimulator.Desktop.Features.TaxProfiles;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;

namespace CashflowSimulator.Desktop.Features.Main.Navigation;

/// <summary>
/// Standard-Implementierung der Navigationsstruktur.
/// </summary>
public class NavigationConfiguration : INavigationConfiguration
{
    public void Configure(NavigationViewModel navigation, Func<Type, object[]?, System.Threading.Tasks.Task> navigateAction, Func<bool> canNavigatePredicate)
    {
        // Hilfsmethode zur Erstellung eines Items
        NavItemViewModel CreateItem(string displayName, Symbol icon, Type targetType, object[]? parameters = null)
        {
            var item = new NavItemViewModel
            {
                DisplayName = displayName,
                Icon = icon,
                IsActive = false
            };
            
            item.Command = new AsyncRelayCommand(async () => 
            {
                await navigateAction(targetType, parameters).ConfigureAwait(true);
                
                // Active-State setzen
                var all = navigation.GetAllItems();
                foreach (var i in all)
                    i.IsActive = (i == item);
            }, canNavigatePredicate);

            return item;
        }

        var szenarioItem = CreateItem("Szenario", Symbol.Database, typeof(MetaEditViewModel));
        var eckdatenItem = CreateItem("Eckdaten", Symbol.Calendar, typeof(EckdatenViewModel));
        var marktdatenItem = CreateItem("Marktdaten", Symbol.ChartMultiple, typeof(MarktdatenViewModel));
        var korrelationenItem = CreateItem("Korrelationen", Symbol.Link, typeof(KorrelationenViewModel));
        var anlageklassenItem = CreateItem("Anlageklassen", Symbol.Grid, typeof(AssetClassesViewModel));
        
        var vermoegenswerteItem = CreateItem("Vermögenswerte", Symbol.Stack, typeof(PortfolioViewModel));
        var transaktionenItem = CreateItem("Transaktionen", Symbol.Document, typeof(TransactionsViewModel));
        
        var laufendeEinnahmenItem = CreateItem("Laufende Einnahmen", Symbol.ArrowUp, typeof(CashflowStreamsViewModel), [CashflowType.Income]);
        var laufendeAusgabenItem = CreateItem("Laufende Ausgaben", Symbol.ArrowDown, typeof(CashflowStreamsViewModel), [CashflowType.Expense]);
        
        var geplanteEinnahmenItem = CreateItem("Geplante Einnahmen", Symbol.CalendarAdd, typeof(CashflowEventsViewModel), [CashflowType.Income]);
        var geplanteAusgabenItem = CreateItem("Geplante Ausgaben", Symbol.CalendarCancel, typeof(CashflowEventsViewModel), [CashflowType.Expense]);
        
        var steuerprofileItem = CreateItem("Steuerprofile", Symbol.Receipt, typeof(TaxProfilesViewModel));
        var strategieprofileItem = CreateItem("Strategieprofile", Symbol.Target, typeof(StrategyProfilesViewModel));
        var allocationProfilesItem = CreateItem("Allokationsprofile", Symbol.Grid, typeof(AllocationProfilesViewModel));
        
        var lebensphasenItem = CreateItem("Lebensphasen", Symbol.Person, typeof(LifecyclePhasesViewModel));
        
        // Use true for Settings (always available regardless of project state?)
        // In original MainShellViewModel, OpenSettingsCommand doesn't have CanExecute.
        var einstellungenItem = new NavItemViewModel
        {
            DisplayName = "Einstellungen",
            Icon = Symbol.Settings,
            IsActive = false
        };
        einstellungenItem.Command = new AsyncRelayCommand(async () => 
        {
            await navigateAction(typeof(SettingsViewModel), null).ConfigureAwait(true);
            var all = navigation.GetAllItems();
            foreach (var i in all) i.IsActive = (i == einstellungenItem);
        });
        
        var analyseItem = new NavItemViewModel
        {
            DisplayName = "Analyse",
            Icon = Symbol.ChartMultiple,
            IsActive = false
        };
        analyseItem.Command = new AsyncRelayCommand(async () => 
        {
            await navigateAction(typeof(AnalysisDashboardViewModel), null).ConfigureAwait(true);
            var all = navigation.GetAllItems();
            foreach (var i in all) i.IsActive = (i == analyseItem);
        }, canNavigatePredicate);

        var groupSzenario = new NavGroupViewModel { DisplayName = "Szenario" };
        groupSzenario.Items.Add(szenarioItem);

        var groupStammdaten = new NavGroupViewModel { DisplayName = "Stammdaten" };
        groupStammdaten.Items.Add(eckdatenItem);
        groupStammdaten.Items.Add(marktdatenItem);
        groupStammdaten.Items.Add(korrelationenItem);
        groupStammdaten.Items.Add(anlageklassenItem);

        var groupVermoegen = new NavGroupViewModel { DisplayName = "Vermögen & Cashflow" };
        groupVermoegen.Items.Add(vermoegenswerteItem);
        groupVermoegen.Items.Add(transaktionenItem);

        var groupEinnahmenAusgaben = new NavGroupViewModel { DisplayName = "Einnahmen & Ausgaben" };
        groupEinnahmenAusgaben.Items.Add(laufendeEinnahmenItem);
        groupEinnahmenAusgaben.Items.Add(laufendeAusgabenItem);
        groupEinnahmenAusgaben.Items.Add(geplanteEinnahmenItem);
        groupEinnahmenAusgaben.Items.Add(geplanteAusgabenItem);

        var groupProfile = new NavGroupViewModel { DisplayName = "Profile" };
        groupProfile.Items.Add(steuerprofileItem);
        groupProfile.Items.Add(strategieprofileItem);
        groupProfile.Items.Add(allocationProfilesItem);

        var groupLebensplanung = new NavGroupViewModel { DisplayName = "Lebensplanung" };
        groupLebensplanung.Items.Add(lebensphasenItem);

        var groupAnalyse = new NavGroupViewModel { DisplayName = "Analyse & Auswertung" };
        groupAnalyse.Items.Add(analyseItem);

        var groupApp = new NavGroupViewModel { DisplayName = "App" };
        groupApp.Items.Add(einstellungenItem);

        navigation.Groups.Add(groupSzenario);
        navigation.Groups.Add(groupStammdaten);
        navigation.Groups.Add(groupVermoegen);
        navigation.Groups.Add(groupEinnahmenAusgaben);
        navigation.Groups.Add(groupProfile);
        navigation.Groups.Add(groupLebensplanung);
        navigation.Groups.Add(groupAnalyse);
        navigation.Groups.Add(groupApp);
    }
}
