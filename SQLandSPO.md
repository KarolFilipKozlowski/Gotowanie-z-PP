# SQLandSPO - synchronizacja danych SQL <-> SPO

**( :star: ) - do uzupełnienia na prezentacji**

## GutHub repos:

- https://github.com/pnp/pnpcore
- https://github.com/pnp/pnpframework
- https://github.com/sadomovalex/camlex

# Koncept aplikacji

Z *xyz* powodu nie możemy skorzystać z innego rozwiązania niż płatny konektor, ale chcemy w cyklu dziennym synchronizować dane między SQL a SPO. 
Pierwsze uruchomienie aplikacji ma wczytać wszystkie dane z bazy do pustej listy SPO.
Każde następne uruchomienie aplikacji (w task scheduler) ma sprawdzać czy w dniu dzisiejszym nie powstał element w SPO i/lub SQL i przesłać go do SQL/SPO.


#### ----
> Power Platform Polska | Gotowanie z PP | 31.05.2023 - > C# a komu to potrzebne - zażądanie danymi w SPO bez LC

:thumbsup: [Karol Kozłowski - Citizen Developer](https://citdev.pl/)