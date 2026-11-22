import { observer } from "mobx-react-lite";
import React, { useEffect, useState } from "react";
import InfiniteScroll from "react-infinite-scroller";
import { Grid, Loader } from "semantic-ui-react";
import { PagingParams } from "../../../app/models/pagination";
import { useStore } from "../../../app/stores/store";
import SmurfFilters from "./SmurfFilters";
import SmurfList from "./SmurfList";
import SmurfListItemPlaceholder from "./SmurfListItemPlaceholder";

export default observer(function SmurfDashboard() {
    const {smurfStore} = useStore();
    const {loadSmurfs, setPagingParams, pagination} = smurfStore;
    const [loadingNext, setLoadingNext] = useState(false);
  
    function handleGetNext() {
        setLoadingNext(true);
        setPagingParams(new PagingParams(pagination!.currentPage + 1))
        loadSmurfs().then(() => setLoadingNext(false));
    }

    useEffect(() => {
      if (!pagination && !smurfStore.loadingInitial) loadSmurfs();
    }, [pagination, loadSmurfs, smurfStore])

    return (
        <Grid>
            <Grid.Column width='10'>
                {smurfStore.loadingInitial && !loadingNext ? (
                    <>
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                        <SmurfListItemPlaceholder />
                    </>
                ) : (
                        <InfiniteScroll
                            pageStart={0}
                            loadMore={handleGetNext}
                            hasMore={!loadingNext && !!pagination && pagination.currentPage < pagination.totalPages}
                            initialLoad={false}
                        >
                            <SmurfList />
                        </InfiniteScroll>
                    )}
            </Grid.Column>
            <Grid.Column width='6'>
                <SmurfFilters />
            </Grid.Column>
            <Grid.Column width={10}>
               <Loader active={loadingNext} />
            </Grid.Column>
        </Grid>
    )
})