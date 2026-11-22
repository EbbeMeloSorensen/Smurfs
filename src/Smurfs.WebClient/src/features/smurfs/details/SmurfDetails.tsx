import { observer } from "mobx-react-lite";
import React, { useEffect } from "react";
import { useParams } from "react-router-dom";
import LoadingComponent from "../../../app/layout/LoadingComponents";
import { useStore } from "../../../app/stores/store";
import SmurfDetailedHeader from "./SmurfDetailedHeader";
import SmurfDetailedInfo from "./SmurfDetailedInfo";

export default observer(function SmurfDetails() {
  const { smurfStore } = useStore();
  const {
    selectedSmurf: smurf,
    loadSmurf,
    loadingInitial,
    clearSelectedSmurf,
  } = smurfStore;
  const { id } = useParams<{ id: string }>();

  useEffect(() => {
    if (id) loadSmurf(id);
    return () => clearSelectedSmurf();
  }, [id, loadSmurf, clearSelectedSmurf]);

  if (loadingInitial || !smurf) return <LoadingComponent />;

  return (
    <>
      <SmurfDetailedHeader smurf={smurf} />
      <SmurfDetailedInfo smurf={smurf} />
    </>
  );
});
